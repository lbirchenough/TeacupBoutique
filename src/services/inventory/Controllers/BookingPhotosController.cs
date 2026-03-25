using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace inventory.Controllers
{
    [Route("api/bookings/{bookingId:guid}/photos")]
    [ApiController]
    public class BookingPhotosController : ControllerBase
    {
        private static readonly HashSet<string> AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

        [HttpPost]
        public async Task<IActionResult> Upload(Guid bookingId, IFormFile file,
            [FromServices] IConfiguration config)
        {
            if (!AllowedMimeTypes.Contains(file.ContentType))
                return BadRequest("Only JPEG, PNG, and WebP images are allowed.");

            var cloudinaryUrl = config["CLOUDINARY_URL"]
                ?? throw new InvalidOperationException("CLOUDINARY_URL is not configured.");

            var cloudinary = new Cloudinary(cloudinaryUrl);
            cloudinary.Api.Secure = true;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = $"bookings/{bookingId}/{Guid.NewGuid()}",
                Folder = "teacup-boutique"
            };

            var result = await cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
                return StatusCode(500, result.Error.Message);

            return Ok(new { url = result.SecureUrl.ToString() });
        }
    }
}
