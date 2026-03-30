using System.Text.Json;
using inventory.Data;
using inventory.Entities;
using Messaging.Interfaces;
using inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController(InventoryDbContext _context, IMessagePublisher _publisher) : ControllerBase
    {
        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking is null) return NotFound();

            if (booking.Status is BookingStatus.CheckedOut or BookingStatus.Returned or BookingStatus.Completed or BookingStatus.Cancelled)
                return BadRequest($"Booking cannot be cancelled in its current state ({booking.Status}).");

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var evt = new BookingCancelledEvent(booking.OrderId, booking.Id);
            await _publisher.PublishAsync("inventory.BookingCancelled", JsonSerializer.Serialize(evt));

            return NoContent();
        }

        [HttpPut("{id:guid}/checkout")]
        public async Task<IActionResult> CheckOut(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking is null) return NotFound();
            if (booking.Status != BookingStatus.Confirmed)
                return BadRequest("Booking must be Confirmed before checking out.");

            booking.Status = BookingStatus.CheckedOut;
            booking.CheckedOutAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:guid}/return")]
        public async Task<IActionResult> MarkReturned(Guid id, [FromBody] MarkReturnedRequest request)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null) return NotFound();
            if (booking.Status != BookingStatus.CheckedOut)
                return BadRequest("Booking must be CheckedOut before marking as returned.");

            foreach (var itemReturn in request.Items)
            {
                var bookingItem = booking.BookingItems!.FirstOrDefault(bi => bi.Id == itemReturn.BookingItemId);
                if (bookingItem is null) continue;

                bookingItem.ReturnCondition = itemReturn.ReturnCondition;
                bookingItem.ReturnNotes = itemReturn.ReturnNotes;
                bookingItem.ReturnedAt = DateTime.UtcNow;
                bookingItem.UpdatedAt = DateTime.UtcNow;
            }

            booking.Status = BookingStatus.Returned;
            booking.ReturnedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteBookingRequest request)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking is null) return NotFound();
            if (booking.Status != BookingStatus.Returned)
                return BadRequest("Booking must be Returned before completing.");

            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.DepositAmountKept = request.DepositAmountKept;
            booking.CompletionNotes = request.CompletionNotes;
            await _context.SaveChangesAsync();

            var evt = new BookingCompletedEvent(
                booking.OrderId,
                booking.Id,
                request.DepositAmountKept,
                request.CompletionNotes,
                request.ReturnPhotoUrls ?? []);
            await _publisher.PublishAsync("inventory.BookingCompleted", JsonSerializer.Serialize(evt));

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .Select(b => new
                {
                    b.Id,
                    b.OrderId,
                    b.ReservationDate,
                    b.Status,
                    b.ReservedAt,
                    b.ConfirmedAt,
                    b.CheckedOutAt,
                    b.ReturnedAt,
                    b.CompletedAt,
                    b.Notes,
                    b.CreatedAt,
                    ItemCount = b.BookingItems!.Count
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.Product)
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.InventoryItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null)
                return NotFound();

            var result = new
            {
                booking.Id,
                booking.OrderId,
                booking.ReservationDate,
                booking.Status,
                booking.ReservedAt,
                booking.ConfirmedAt,
                booking.CheckedOutAt,
                booking.ReturnedAt,
                booking.CompletedAt,
                booking.Notes,
                booking.CreatedAt,
                BookingItems = booking.BookingItems!.Select(bi => new
                {
                    bi.Id,
                    bi.ReservationDate,
                    bi.ReturnCondition,
                    bi.ReturnNotes,
                    bi.ReturnedAt,
                    bi.CompletedAt,
                    ProductId = bi.ProductId,
                    ProductName = bi.Product!.Name,
                    ProductColour = bi.Product.Colour,
                    InventoryItemId = bi.InventoryItemId,
                })
            };

            return Ok(result);
        }
    }

    public record BookingItemReturnDto(Guid BookingItemId, ReturnCondition ReturnCondition, string? ReturnNotes);
    public record MarkReturnedRequest(List<BookingItemReturnDto> Items);
    public record CompleteBookingRequest(decimal? DepositAmountKept, string? CompletionNotes, List<string>? ReturnPhotoUrls);
}
