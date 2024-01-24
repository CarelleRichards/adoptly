namespace Adoptly.Web.Models;

public class UploadContentRequest
{
	public string Content { get; set; }

    // What the file should be called in blob storage.
    public string FileName { get; set; }

    // The container the blob should be in. 
    public string ContainerName { get; set; }
}