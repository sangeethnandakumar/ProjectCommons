# Allows management Of Azure Storages to Upload/Download/Info on Files

## Step 1: IAzureStorageService
```cs
using Azure.Storage.Blobs.Models;

namespace parinaybharat.api.application
{
    public interface IAzureStorageService
    {
        Task UploadFileAsync(string containerName, string directory, string fileName, Stream fileStream,
            Action onStarted,
            Action<long> onUploading,
            Action onCompleted,
            Action<Exception> onFailed);
        Task DownloadFileAsync(string containerName, string filePath,
            Action onStarted,
            Action<Stream> onDownloading,
            Action<byte[]> onCompleted,
            Action<Exception> onFailed);
        Task RenameAsync(string containerName, string oldName, string newName, bool isDirectory);
        Task DeleteAsync(string containerName, string path, bool isDirectory);
        Task CopyAsync(string sourceContainer, string sourcePath, string destContainer, string destPath);
        Task MoveAsync(string sourceContainer, string sourcePath, string destContainer, string destPath);
        Task<FileInfo> GetFileInfoAsync(string containerName, string filePath);
        Task MoveToTierAsync(string containerName, string filePath, AccessTier tier);
    }

    public class FileInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
    }
}

```

## Step 2: AzureStorageService
```cs
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using parinaybharat.api.application;
using FileInfo = parinaybharat.api.application.FileInfo;

public class AzureStorageService : IAzureStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureStorageService(ISecretProvider secretProvider)
    {
        var connString = secretProvider.GetSecretAsync("conn-storage").Result;
        _blobServiceClient = new BlobServiceClient(connString);
    }

    public async Task UploadFileAsync(string containerName, string directory, string fileName, Stream fileStream,
        Action onStarted,
        Action<long> onUploading,
        Action onCompleted,
        Action<Exception> onFailed)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient($"{directory}/{fileName}");

            onStarted?.Invoke();

            var totalBytes = fileStream.Length;
            var uploadedBytes = 0L;
            var buffer = new byte[81920];  // 80KB buffer
            int bytesRead;

            // Use a memory stream to track the progress
            using (var memoryStream = new MemoryStream())
            {
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await memoryStream.WriteAsync(buffer, 0, bytesRead);
                    uploadedBytes += bytesRead;

                    // Invoke the onUploading callback with progress
                    onUploading?.Invoke(uploadedBytes);
                }

                // Upload the file
                memoryStream.Position = 0; // Reset position to start for uploading
                await blobClient.UploadAsync(memoryStream, overwrite: true);
            }

            // Invoke the onCompleted callback when upload finishes
            onCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            // Invoke the onFailed callback if an error occurs
            onFailed?.Invoke(ex);
        }
    }


    public async Task DownloadFileAsync(string containerName, string filePath,
        Action onStarted,
        Action<Stream> onDownloading,
        Action<byte[]> onCompleted,
        Action<Exception> onFailed)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            onStarted?.Invoke();

            var downloadResponse = await blobClient.DownloadAsync();

            using (var memoryStream = new MemoryStream())
            {
                // Invoke the onDownloading callback with the stream during download
                onDownloading?.Invoke(downloadResponse.Value.Content);

                // Copy the stream content into a MemoryStream
                await downloadResponse.Value.Content.CopyToAsync(memoryStream);

                // Convert stream content to byte array after completion
                byte[] result = memoryStream.ToArray();

                // Invoke the onCompleted callback with the byte array
                onCompleted?.Invoke(result);
            }
        }
        catch (Exception ex)
        {
            onFailed?.Invoke(ex);
        }
    }


    public async Task RenameAsync(string containerName, string oldName, string newName, bool isDirectory)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        if (isDirectory)
        {
            // Logic to handle renaming directory (copy then delete)
        }
        else
        {
            var sourceBlob = containerClient.GetBlobClient(oldName);
            var destBlob = containerClient.GetBlobClient(newName);

            await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
            await sourceBlob.DeleteAsync();
        }
    }

    public async Task DeleteAsync(string containerName, string path, bool isDirectory)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        if (isDirectory)
        {
            // Logic to delete directory (list all blobs under the directory and delete)
        }
        else
        {
            var blobClient = containerClient.GetBlobClient(path);
            await blobClient.DeleteIfExistsAsync();
        }
    }

    public async Task CopyAsync(string sourceContainer, string sourcePath, string destContainer, string destPath)
    {
        var sourceClient = _blobServiceClient.GetBlobContainerClient(sourceContainer).GetBlobClient(sourcePath);
        var destClient = _blobServiceClient.GetBlobContainerClient(destContainer).GetBlobClient(destPath);

        await destClient.StartCopyFromUriAsync(sourceClient.Uri);
    }

    public async Task MoveAsync(string sourceContainer, string sourcePath, string destContainer, string destPath)
    {
        await CopyAsync(sourceContainer, sourcePath, destContainer, destPath);
        await DeleteAsync(sourceContainer, sourcePath, isDirectory: false);
    }

    public async Task<FileInfo> GetFileInfoAsync(string containerName, string filePath)
    {
        var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(filePath);
        var properties = await blobClient.GetPropertiesAsync();

        return new FileInfo
        {
            Name = blobClient.Name,
            Size = properties.Value.ContentLength,
            ContentType = properties.Value.ContentType
        };
    }

    public async Task MoveToTierAsync(string containerName, string filePath, AccessTier tier)
    {
        var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(filePath);
        await blobClient.SetAccessTierAsync(tier);
    }
}
```
