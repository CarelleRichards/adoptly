using Adoptly.Web.Managers;
using Adoptly.Web.Models.Enums;
using ImageMagick;

namespace Adoptly.Web.Services;

public class FileService
{
    private readonly BlobManager _blobManager;
    private static readonly string[] AllowedFileExtensions = { ".jpg", ".jpeg", ".png", ".heic" };

    public FileService(BlobManager blobManager) => _blobManager = blobManager;

    public async Task<bool> UploadFile(IFormFile imageFile, string fileName, BlobContainers blobContainer)
    {
        // If the file extension does not match the allowed extensions, throw an IO exception. 
        
        if (!VerifyFileExtension(imageFile))
            throw new IOException();
        
        // Try and return from the function if any exception occurs while uploading the file to Azure's blob storage.
        
        try
        {
            // Read the image file using OpenReadStream and create a MagickImage object.
            
            await using var stream = imageFile.OpenReadStream();
            using MagickImage image = new(stream);
            
            // Set up MagickImage dependencies and create an image.
            
            image.Format = MagickFormat.Jpg;
            image.Resize(new MagickGeometry(400, 400));
            image.Quality = 100;
            image.Alpha(AlphaOption.Remove);
            image.BackgroundColor = new MagickColor("#FFFFFF");
            var imageData = image.ToByteArray();

            // Upload the image to Azure's blob storage. 
            
            await _blobManager.UploadContentBlobAsync(Convert.ToBase64String(imageData), fileName,
                GetContainerName(blobContainer));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteFile(string fileName, BlobContainers blobContainer)
    {
        // Delete an image from Azure's blob storage.
        
        await _blobManager.DeleteBlobAsync(fileName, GetContainerName(blobContainer));
        return true;
    }

    private static bool VerifyFileExtension(IFormFile imageFile)
    {
        // Get file extenstion from a file and store it.
        
        string extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        
        // Return if the file extension is not null and belongs to the list of allowed file extensions.
        
        return !string.IsNullOrEmpty(extension) && AllowedFileExtensions.Contains(extension);
    }

    private static string GetContainerName(BlobContainers blobContainer)
    {
        // Return the blob container name based on the container enum.
        
        return blobContainer switch
        {
            BlobContainers.AdminProfilePictures => "admin-profile-pictures",
            BlobContainers.AdopterProfilePictures => "adopter-profile-pictures",
            BlobContainers.ShelterProfilePictures => "shelter-profile-pictures",
            BlobContainers.PetProfilePictures => "pet-profile-pictures",
            _ => null
        };
    }
}