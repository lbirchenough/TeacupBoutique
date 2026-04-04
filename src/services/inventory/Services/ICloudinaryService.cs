namespace inventory.Services;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadAsync(IFormFile file, string folder);
    Task DeleteAsync(string publicId);
}

public record CloudinaryUploadResult(string Url, string PublicId);
