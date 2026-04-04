using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace inventory.Services;

public class CloudinaryService(IConfiguration config) : ICloudinaryService
{
    private static readonly HashSet<string> AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    private Cloudinary CreateClient()
    {
        var url = config["CLOUDINARY_URL"]
            ?? throw new InvalidOperationException("CLOUDINARY_URL is not configured.");
        var cloudinary = new Cloudinary(url);
        cloudinary.Api.Secure = true;
        return cloudinary;
    }

    public async Task<CloudinaryUploadResult> UploadAsync(IFormFile file, string folder)
    {
        if (!AllowedMimeTypes.Contains(file.ContentType))
            throw new ArgumentException("Only JPEG, PNG, and WebP images are allowed.");

        var cloudinary = CreateClient();
        using var stream = file.OpenReadStream();
        var result = await cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = $"{folder}/{Guid.NewGuid()}",
            Folder = "teacup-boutique"
        });

        if (result.Error is not null)
            throw new Exception(result.Error.Message);

        return new CloudinaryUploadResult(result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteAsync(string publicId)
    {
        var cloudinary = CreateClient();
        var result = await cloudinary.DestroyAsync(new DeletionParams(publicId));
        if (result.Error is not null)
            throw new Exception(result.Error.Message);
    }
}
