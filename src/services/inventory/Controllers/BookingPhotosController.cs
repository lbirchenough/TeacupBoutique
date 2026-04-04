using inventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace inventory.Controllers
{
    [Route("api/bookings/{bookingId:guid}/photos")]
    [ApiController]
    public class BookingPhotosController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Upload(Guid bookingId, IFormFile file,
            [FromServices] ICloudinaryService cloudinary)
        {
            try
            {
                var result = await cloudinary.UploadAsync(file, $"bookings/{bookingId}");
                return Ok(new { url = result.Url });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
