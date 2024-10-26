
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
```
