
## Step 1: Create KeyVault & Add RBAC Roles
![image](https://github.com/user-attachments/assets/b1cba0cd-26ee-4608-a7b7-37aa7e5fcf90)

## Step 2: Register App in Azure App If Managed Identity Is Not Used
![image](https://github.com/user-attachments/assets/6335ba72-905d-4cf9-b61a-69d2e3a06a48)

## Step 3: Access Secret Contents
```cs
  var tenantId = "*****************";
  var clientId = "*****************"; //App ID in Azure App Registrations
  var clientSecret = "******************"; //Create client credentials
  var keyVaultUri = new Uri("https://parinaybharat.vault.azure.net/");

  // Authenticate using ClientSecretCredential
  var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
  var secretClient = new SecretClient(vaultUri: keyVaultUri, credential: clientSecretCredential);

  // Access a secret
  var secret = await secretClient.GetSecretAsync("conn-mongodb");
  var secretValue = secret.Value.Value;
```
