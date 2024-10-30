
## Step 1: Create KeyVault & Add RBAC Roles
![image](https://github.com/user-attachments/assets/b1cba0cd-26ee-4608-a7b7-37aa7e5fcf90)

## Step 2: Register App in Azure App If Managed Identity Is Not Used
![image](https://github.com/user-attachments/assets/6335ba72-905d-4cf9-b61a-69d2e3a06a48)

## Step 3: Create KeyVault Secrets in appsettings.json
```json
{
  "KeyVault": {
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    "VaultUri": "",
    "CacheExpirationMinutes": 60
  }
}
```

## Step 4: Create Binding Model
```cs
    public class KeyVaultOptions
    {
        public const string KeyVault = "parinaybharat ";
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string VaultUri { get; set; }
        public int CacheExpirationMinutes { get; set; } = 60;
    }
```

## Step 5: Bind options and add MemoryCache in DI
```cs
            //Bind Options
            services.Configure<KeyVaultOptions>(configuration.GetSection("KeyVault"));

            //Add Memory Cache
            services.AddMemoryCache();

            //KeyVault Service
            services.AddSingleton<ISecretProvider, SecretProvider>();
```

## Step 6: Create ISecretProvider
```cs
public interface ISecretProvider
{
    Task<string> GetSecretAsync(string secretName);
}
```

## Step 7: Create SecretProvider
```cs
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace parinaybharat.api.application.Helpers.Secrets
{
    public class SecretProvider : ISecretProvider
    {
        private readonly SecretClient _secretClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SecretProvider> _logger;
        private readonly TimeSpan _cacheExpiration;

        public SecretProvider(IOptions<KeyVaultOptions> options, IMemoryCache cache, ILogger<SecretProvider> logger)
        {
            var keyVaultOptions = options.Value;
            _cache = cache;
            _logger = logger;

            _secretClient = new SecretClient(
                new Uri(keyVaultOptions.VaultUri),
                new ClientSecretCredential(keyVaultOptions.TenantId, keyVaultOptions.ClientId, keyVaultOptions.ClientSecret)
            );
            _cacheExpiration = TimeSpan.FromMinutes(keyVaultOptions.CacheExpirationMinutes);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            var cacheKey = $"KeyVault_{secretName}";

            if (_cache.TryGetValue(cacheKey, out string cachedSecret)) return cachedSecret;

            _logger.LogWarning("Cache miss for {secretName}. Auto fetching from KeyVault", secretName);

            try
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                var secretValue = secret.Value.Value;

                _cache.Set(cacheKey, secretValue, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheExpiration));

                return secretValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed retrieving secret {secretName}", secretName);
                throw;
            }
        }
    }
}
```

## Step 8: Use Anywhere. When Configuring DBContexts use Scoped
```cs
// Remove any DbContext pooling/caching to ensure we always use SecretProvider's cache
services.AddDbContext<AppDBContext>((sp, options) =>
{
    var secretProvider = sp.GetRequiredService<ISecretProvider>();
    var connectionString = secretProvider.GetSecretAsync("conn-mongodb").GetAwaiter().GetResult();
    options.UseMongoDB(connectionString, "ParinayBharat");
}, ServiceLifetime.Scoped);

services.AddScoped<IAppDBContext>(sp => sp.GetRequiredService<AppDBContext>());
```

## Step 9: Multiple DBContext Scenerios with DbContextBFactory
```cs
//To be discussed
```

## Unit Tests
```cs
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace parinaybharat.api.application.Helpers.Secrets.Tests
{
    public class SecretProviderTests
    {
        private readonly Mock<IOptions<KeyVaultOptions>> _mockOptions;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<SecretProvider>> _mockLogger;
        private readonly Mock<SecretClient> _mockSecretClient;
        private readonly KeyVaultOptions _keyVaultOptions;
        
        public SecretProviderTests()
        {
            _mockOptions = new Mock<IOptions<KeyVaultOptions>>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<SecretProvider>>();
            _mockSecretClient = new Mock<SecretClient>();
            
            _keyVaultOptions = new KeyVaultOptions
            {
                VaultUri = "https://test-vault.vault.azure.net/",
                TenantId = "test-tenant",
                ClientId = "test-client",
                ClientSecret = "test-secret",
                CacheExpirationMinutes = 5
            };
            
            _mockOptions.Setup(x => x.Value).Returns(_keyVaultOptions);
        }

        [Fact]
        public async Task GetSecretAsync_WhenSecretExistsInCache_ReturnsFromCache()
        {
            // Arrange
            const string secretName = "test-secret";
            const string expectedSecret = "cached-secret-value";
            var cacheKey = $"KeyVault_{secretName}";

            _mockCache.Setup(x => x.TryGetValue(cacheKey, out expectedSecret))
                     .Returns(true);

            var secretProvider = new SecretProvider(_mockOptions.Object, _mockCache.Object, _mockLogger.Object);

            // Act
            var result = await secretProvider.GetSecretAsync(secretName);

            // Assert
            Assert.Equal(expectedSecret, result);
            _mockCache.Verify(x => x.TryGetValue(cacheKey, out expectedSecret), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never);
        }

        [Fact]
        public async Task GetSecretAsync_WhenSecretNotInCache_FetchesFromKeyVaultAndCaches()
        {
            // Arrange
            const string secretName = "test-secret";
            const string expectedSecret = "vault-secret-value";
            var cacheKey = $"KeyVault_{secretName}";
            string outputSecret = null;

            _mockCache.Setup(x => x.TryGetValue(cacheKey, out outputSecret))
                     .Returns(false);

            var response = Response.FromValue(
                new KeyVaultSecret(secretName, expectedSecret),
                Mock.Of<Response>());

            _mockSecretClient.Setup(x => x.GetSecretAsync(secretName, null, default))
                           .ReturnsAsync(response);

            var cacheEntryMock = new Mock<ICacheEntry>();
            _mockCache.Setup(x => x.CreateEntry(cacheKey))
                     .Returns(cacheEntryMock.Object);

            var secretProvider = new SecretProvider(_mockOptions.Object, _mockCache.Object, _mockLogger.Object);

            // Act
            var result = await secretProvider.GetSecretAsync(secretName);

            // Assert
            Assert.Equal(expectedSecret, result);
            _mockCache.Verify(x => x.TryGetValue(cacheKey, out outputSecret), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(secretName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public async Task GetSecretAsync_WhenKeyVaultThrowsException_LogsErrorAndRethrows()
        {
            // Arrange
            const string secretName = "test-secret";
            var cacheKey = $"KeyVault_{secretName}";
            string outputSecret = null;
            var expectedException = new RequestFailedException("Key Vault Error");

            _mockCache.Setup(x => x.TryGetValue(cacheKey, out outputSecret))
                     .Returns(false);

            _mockSecretClient.Setup(x => x.GetSecretAsync(secretName, null, default))
                           .ThrowsAsync(expectedException);

            var secretProvider = new SecretProvider(_mockOptions.Object, _mockCache.Object, _mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RequestFailedException>(
                () => secretProvider.GetSecretAsync(secretName)
            );

            Assert.Same(expectedException, exception);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(secretName)),
                It.IsAny<RequestFailedException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }
    }
}
```
