using Microsoft.AspNetCore.StaticFiles;

namespace Adoptly.Web.Extensions;

public static class FileExtensions
{
	private static readonly FileExtensionContentTypeProvider Provider = new();

    // Tries to get the file extension content type provided.
    // If file extension isn't recognisable, default is used.

    public static string GetContentType(this string fileName)
	{
		if (!Provider.TryGetContentType(fileName, out var contentType))
			contentType = "application/octet-stream";
		return contentType;
	}
}