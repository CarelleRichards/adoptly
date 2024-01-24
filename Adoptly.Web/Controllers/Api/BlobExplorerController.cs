using Microsoft.AspNetCore.Mvc;
using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Filters;

namespace Adoptly.Web.Controllers.Api;

// API controller for Azure Blob Storage. 

[Route("blobs")]
[ApiController]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class BlobExplorerController : ControllerBase
{
	private readonly BlobManager _blobManager;

	public BlobExplorerController(BlobManager blobManager) => _blobManager = blobManager;

    // Get a blob from a container.

	[HttpGet("{containerName}/{blobName}")]
	public async Task<IActionResult> GetBlob(string containerName, string blobName)
	{
		BlobInfo data = await _blobManager.GetBlobAsync(blobName, containerName);
        return File(data.Content, data.ContentType);
	}

    // Get a list of all blobs in a container.

    [HttpGet("list/{containerName}")]
    public async Task<IActionResult> ListBlobs(string containerName) =>
        Ok(await _blobManager.ListBlobsAsync(containerName));

    // List all containers.

    [HttpGet("list")]
    public async Task<IActionResult> ListContainers() =>
        Ok(await _blobManager.ListContainersAsync());

    [HttpPost("{containerName}")]
    public async Task<IActionResult> CreateContainer(string containerName)
    {
        await _blobManager.CreateContainerAsync(containerName);
        return Ok();
    }

    // Upload a blob from your local directory. For developer use only. 

    [HttpPost("uploadFile")]
    public async Task<IActionResult> UploadFile([FromBody] UploadFileRequest request)
    {
        await _blobManager.UploadFileBlobAsync(request.FilePath, request.FileName, request.ContainerName);
        return Ok();
    }

    // Upload a blob from memory.

    [HttpPost("uploadContent")]
    public async Task<IActionResult> UploadContent([FromBody] UploadContentRequest request)
    {
        await _blobManager.UploadContentBlobAsync(request.Content, request.FileName, request.ContainerName);
        return Ok();
    }

    // Delete a blob from a container.

    [AuthorizeAnyUser]
    [HttpDelete("{containerName}/{blobName}")]
    public async Task<IActionResult> DeleteFile(string containerName, string blobName)
    {
        await _blobManager.DeleteBlobAsync(blobName, containerName);
        return Ok();
    }
}