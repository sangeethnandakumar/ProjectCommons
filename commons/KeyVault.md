
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
using System.Threading.Tasks;
using Xunit;

namespace parinaybharat.api.application.Helpers.Secrets.Tests
{
    public class SecretProviderTests
    {
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<SecretProvider>> _mockLogger;
        private readonly SecretProvider _secretProvider;
        private readonly Mock<SecretClient> _mockSecretClient;

        public SecretProviderTests()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<SecretProvider>>();
            
            var options = Mock.Of<IOptions<KeyVaultOptions>>(opt => 
                opt.Value == new KeyVaultOptions 
                { 
                    VaultUri = "https://test.vault.azure.net",
                    TenantId = "tenant",
                    ClientId = "client",
                    ClientSecret = "secret",
                    CacheExpirationMinutes = 5
                });

            _mockSecretClient = new Mock<SecretClient>(new Uri("https://test.vault.azure.net"), 
                new Mock<Azure.Core.TokenCredential>().Object);
                
            _secretProvider = new SecretProvider(options, _mockCache.Object, _mockLogger.Object);
            
            // Set mock secret client using reflection
            typeof(SecretProvider)
                .GetField("_secretClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_secretProvider, _mockSecretClient.Object);
        }

        [Fact]
        public async Task GetSecretAsync_WhenInCache_ReturnsFromCache()
        {
            // Arrange
            const string secretName = "cached-secret";
            const string expectedValue = "secret-value";
            object cachedValue = expectedValue;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue)).Returns(true);

            // Act
            var result = await _secretProvider.GetSecretAsync(secretName);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task GetSecretAsync_WhenNotInCache_FetchesAndCaches()
        {
            // Arrange
            const string secretName = "new-secret";
            const string secretValue = "vault-value";
            object cachedValue = null;
            
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue)).Returns(false);
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                     .Returns(Mock.Of<ICacheEntry>());
            
            _mockSecretClient.Setup(x => x.GetSecretAsync(secretName, null, default))
                           .ReturnsAsync(Response.FromValue(
                               new KeyVaultSecret(secretName, secretValue), 
                               Mock.Of<Response>()));

            // Act
            var result = await _secretProvider.GetSecretAsync(secretName);

            // Assert
            Assert.Equal(secretValue, result);
        }

        [Fact]
        public async Task GetSecretAsync_WhenKeyVaultFails_ThrowsException()
        {
            // Arrange
            const string secretName = "error-secret";
            object cachedValue = null;
            var expectedError = new RequestFailedException("Vault error");
            
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue)).Returns(false);
            _mockSecretClient.Setup(x => x.GetSecretAsync(secretName, null, default))
                           .ThrowsAsync(expectedError);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RequestFailedException>(
                () => _secretProvider.GetSecretAsync(secretName));
            Assert.Same(expectedError, exception);
        }
    }
}
```
