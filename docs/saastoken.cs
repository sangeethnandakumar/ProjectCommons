using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Azure.Identity;
using System;
using System.Threading.Tasks;

/// <summary>
/// Helper class for Azure Blob Storage operations and SAS token generation
/// </summary>
public class AzureBlobSasHelper
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly bool _useManagedIdentity;

    /// <summary>
    /// Initialize with connection string
    /// </summary>
    /// <param name="connectionString">Azure Storage connection string</param>
    public AzureBlobSasHelper(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        _blobServiceClient = new BlobServiceClient(connectionString);
        _useManagedIdentity = false;
    }

    /// <summary>
    /// Initialize with managed identity
    /// </summary>
    /// <param name="storageAccountName">Storage account name</param>
    /// <param name="useManagedIdentity">Set to true to use managed identity</param>
    public AzureBlobSasHelper(string storageAccountName, bool useManagedIdentity = true)
    {
        if (string.IsNullOrEmpty(storageAccountName))
            throw new ArgumentException("Storage account name cannot be null or empty", nameof(storageAccountName));

        if (!useManagedIdentity)
            throw new ArgumentException("Use the connection string constructor for non-managed identity scenarios");

        var serviceUri = new Uri($"https://{storageAccountName}.blob.core.windows.net");
        _blobServiceClient = new BlobServiceClient(serviceUri, new DefaultAzureCredential());
        _useManagedIdentity = true;
    }

    /// <summary>
    /// Generate a SAS token URL for a blob
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name (file path)</param>
    /// <param name="expiryHours">SAS token expiry in hours (default: 24 hours)</param>
    /// <param name="permissions">SAS permissions (default: Read)</param>
    /// <returns>SAS token URL</returns>
    public async Task<string> GenerateSasTokenUrlAsync(
        string containerName, 
        string blobName, 
        int expiryHours = 24,
        BlobSasPermissions permissions = BlobSasPermissions.Read)
    {
        try
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);

            // Check if blob exists
            var exists = await blobClient.ExistsAsync();
            if (!exists.Value)
            {
                throw new InvalidOperationException($"Blob '{blobName}' not found in container '{containerName}'");
            }

            // For managed identity, we need to use user delegation key
            if (_useManagedIdentity)
            {
                return await GenerateSasWithUserDelegationKeyAsync(blobClient, expiryHours, permissions);
            }
            else
            {
                return GenerateSasWithAccountKey(blobClient, expiryHours, permissions);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate SAS token: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generate SAS token URL from a full blob URL
    /// </summary>
    /// <param name="blobUrl">Full blob URL (e.g., https://account.blob.core.windows.net/container/blob)</param>
    /// <param name="expiryHours">SAS token expiry in hours (default: 24 hours)</param>
    /// <param name="permissions">SAS permissions (default: Read)</param>
    /// <returns>SAS token URL</returns>
    public async Task<string> GenerateSasTokenUrlFromUrlAsync(
        string blobUrl, 
        int expiryHours = 24,
        BlobSasPermissions permissions = BlobSasPermissions.Read)
    {
        if (string.IsNullOrEmpty(blobUrl))
            throw new ArgumentException("Blob URL cannot be null or empty", nameof(blobUrl));

        var uri = new Uri(blobUrl);
        var pathSegments = uri.AbsolutePath.Trim('/').Split('/');
        
        if (pathSegments.Length < 2)
            throw new ArgumentException("Invalid blob URL format. Expected format: https://account.blob.core.windows.net/container/blob");

        var containerName = pathSegments[0];
        var blobName = string.Join("/", pathSegments, 1, pathSegments.Length - 1);

        return await GenerateSasTokenUrlAsync(containerName, blobName, expiryHours, permissions);
    }

    /// <summary>
    /// Generate SAS token using user delegation key (for managed identity)
    /// </summary>
    private async Task<string> GenerateSasWithUserDelegationKeyAsync(
        BlobClient blobClient, 
        int expiryHours, 
        BlobSasPermissions permissions)
    {
        var delegationKeyStart = DateTimeOffset.UtcNow;
        var delegationKeyExpiry = DateTimeOffset.UtcNow.AddHours(expiryHours);

        // Get user delegation key
        var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(
            delegationKeyStart, 
            delegationKeyExpiry);

        // Create SAS builder
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b", // "b" for blob
            StartsOn = delegationKeyStart,
            ExpiresOn = delegationKeyExpiry
        };

        sasBuilder.SetPermissions(permissions);

        // Generate SAS token
        var sasToken = sasBuilder.ToSasQueryParameters(userDelegationKey, _blobServiceClient.AccountName);

        return $"{blobClient.Uri}?{sasToken}";
    }

    /// <summary>
    /// Generate SAS token using account key (for connection string)
    /// </summary>
    private string GenerateSasWithAccountKey(
        BlobClient blobClient, 
        int expiryHours, 
        BlobSasPermissions permissions)
    {
        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("Cannot generate SAS URI. Ensure you're using a connection string with account key.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(expiryHours)
        };

        sasBuilder.SetPermissions(permissions);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    /// <summary>
    /// Check if a blob exists
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name</param>
    /// <returns>True if blob exists</returns>
    public async Task<bool> BlobExistsAsync(string containerName, string blobName)
    {
        try
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
            var response = await blobClient.ExistsAsync();
            return response.Value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get blob properties
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name</param>
    /// <returns>Blob properties</returns>
    public async Task<BlobProperties> GetBlobPropertiesAsync(string containerName, string blobName)
    {
        var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        var response = await blobClient.GetPropertiesAsync();
        return response.Value;
    }
}

/// <summary>
/// Extension methods for easier usage
/// </summary>
public static class AzureBlobSasHelperExtensions
{
    /// <summary>
    /// Generate SAS token URL for Power BI files with extended permissions
    /// </summary>
    /// <param name="helper">AzureBlobSasHelper instance</param>
    /// <param name="blobUrl">Full blob URL</param>
    /// <param name="expiryHours">Expiry in hours (default: 48 hours for Power BI)</param>
    /// <returns>SAS token URL suitable for Power BI</returns>
    public static async Task<string> GeneratePowerBiSasTokenUrlAsync(
        this AzureBlobSasHelper helper, 
        string blobUrl, 
        int expiryHours = 48)
    {
        // Power BI typically needs Read permissions
        return await helper.GenerateSasTokenUrlFromUrlAsync(
            blobUrl, 
            expiryHours, 
            BlobSasPermissions.Read);
    }
}

// Example usage class
public class Example
{
    public async Task DemonstrateUsage()
    {
        // Example 1: Using connection string
        var helperWithConnectionString = new AzureBlobSasHelper(
            "DefaultEndpointsProtocol=https;AccountName=your_account;AccountKey=your_key;EndpointSuffix=core.windows.net"
        );

        // Example 2: Using managed identity
        var helperWithManagedIdentity = new AzureBlobSasHelper("your_storage_account_name", useManagedIdentity: true);

        // Generate SAS token from your example URL
        string blobUrl = "https://freetoolsfiles.blob.core.windows.net/powerbi/MemberHccGapAnalytics/Report1/MA/v1.pbix";
        
        try
        {
            // Generate SAS token URL (24 hours expiry)
            string sasUrl = await helperWithConnectionString.GenerateSasTokenUrlFromUrlAsync(blobUrl, expiryHours: 24);
            Console.WriteLine($"SAS URL: {sasUrl}");

            // Generate Power BI specific SAS token URL (48 hours expiry)
            string powerBiSasUrl = await helperWithConnectionString.GeneratePowerBiSasTokenUrlAsync(blobUrl);
            Console.WriteLine($"Power BI SAS URL: {powerBiSasUrl}");

            // Check if blob exists
            bool exists = await helperWithConnectionString.BlobExistsAsync("powerbi", "MemberHccGapAnalytics/Report1/MA/v1.pbix");
            Console.WriteLine($"Blob exists: {exists}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}


//-----------------------------

// Using connection string
var helper = new AzureBlobSasHelper("your_connection_string");

// Using managed identity
var helper = new AzureBlobSasHelper("your_storage_account_name", useManagedIdentity: true);

// Generate SAS URL for your Power BI file
string blobUrl = "https://freetoolsfiles.blob.core.windows.net/powerbi/MemberHccGapAnalytics/Report1/MA/v1.pbix";
string sasUrl = await helper.GenerateSasTokenUrlFromUrlAsync(blobUrl, expiryHours: 24);

// Power BI specific method
string powerBiSasUrl = await helper.GeneratePowerBiSasTokenUrlAsync(blobUrl);

//-----------------------------

<PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
<PackageReference Include="Azure.Identity" Version="1.10.4" />
