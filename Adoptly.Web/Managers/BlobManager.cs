using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Adoptly.Web.Extensions;
using BlobInfo = Adoptly.Web.Models.BlobInfo;

namespace Adoptly.Web.Managers;

// Manager for Azure Blob Storage.

public class BlobManager
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobManager(BlobServiceClient blobServiceClient) => _blobServiceClient = blobServiceClient;

    // Get a blob.

    public async Task<BlobInfo> GetBlobAsync(string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var blobDownloadInfo = await blobClient.DownloadAsync();

        return new BlobInfo
        {
            Content = blobDownloadInfo.Value.Content,
            ContentType = blobDownloadInfo.Value.ContentType
        };
    }

    // Lists all containers.

    public async Task<IEnumerable<string>> ListContainersAsync()
    {
        var items = new List<string>();

        await foreach (var container in _blobServiceClient.GetBlobContainersAsync())
            items.Add(container.Name);

        return items;
    }

    // Lists all blobs in the specified container.

    public async Task<IEnumerable<string>> ListBlobsAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var items = new List<string>();

        await foreach (var blobItem in containerClient.GetBlobsAsync())
            items.Add(blobItem.Name);

        return items;
    }

    // Upload file from local directory.

    public async Task UploadFileBlobAsync(string filePath, string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(filePath, new BlobHttpHeaders { ContentType = filePath.GetContentType() });
    }

    // Upload file from memory.

    public async Task UploadContentBlobAsync(string content, string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        var bytes = Convert.FromBase64String(content);
        using var memoryStream = new MemoryStream(bytes);
        await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = fileName.GetContentType() });
    }

    // Delete a blob.

    public async Task DeleteBlobAsync(string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    // Create a conainer for blobs.

    public async Task CreateContainerAsync(string containerName)
    {
        await _blobServiceClient.CreateBlobContainerAsync(containerName);
    }

    // Delete a container.

    public async Task DeleteContainerAsync(string containerName)
    {
        await _blobServiceClient.DeleteBlobContainerAsync(containerName);
    }
}