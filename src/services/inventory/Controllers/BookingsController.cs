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
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.ProductSet)
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.Components!)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null) return NotFound();
            if (booking.Status != BookingStatus.CheckedOut)
                return BadRequest("Booking must be CheckedOut before marking as returned.");

            var setItemIds = request.Items
                .SelectMany(i => i.Components.Select(c => c.SetItemId))
                .Distinct()
                .ToList();

            var setItems = await _context.SetItems
                .Where(si => setItemIds.Contains(si.Id))
                .ToDictionaryAsync(si => si.Id);

            foreach (var itemAssessment in request.Items)
            {
                var bookingItem = booking.BookingItems!.FirstOrDefault(bi => bi.Id == itemAssessment.BookingItemId);
                if (bookingItem is null) continue;

                foreach (var component in itemAssessment.Components)
                {
                    var snapshot = bookingItem.Components?.FirstOrDefault(c => c.SetItemId == component.SetItemId);
                    if (snapshot is null)
                        return BadRequest($"BookingItemComponent for SetItem {component.SetItemId} not found.");

                    var total = component.QuantityGood + component.QuantityDamaged + component.QuantityMissing;
                    if (total != snapshot.Quantity)
                        return BadRequest($"Quantities for {snapshot.Name} must sum to {snapshot.Quantity} (got {total}).");

                    _context.ReturnAssessments.Add(new ReturnAssessment
                    {
                        BookingItemId = bookingItem.Id,
                        SetItemId = component.SetItemId,
                        QuantityGood = component.QuantityGood,
                        QuantityDamaged = component.QuantityDamaged,
                        QuantityMissing = component.QuantityMissing
                    });
                }

                bookingItem.ReturnNotes = itemAssessment.ReturnNotes;
                bookingItem.ReturnedAt = DateTime.UtcNow;
                bookingItem.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var bookingItem in booking.BookingItems!)
            {
                if (bookingItem.ProductSet is not null)
                {
                    bookingItem.ProductSet.Status = Status.Maintenance;
                    bookingItem.ProductSet.CleanedAt = null;
                }
            }

            booking.Status = BookingStatus.Returned;
            booking.ReturnedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            decimal totalDeduction = request.Items
                .SelectMany(i => i.Components)
                .Sum(c =>
                {
                    var snapshot = booking.BookingItems!
                        .SelectMany(bi => bi.Components ?? [])
                        .FirstOrDefault(comp => comp.SetItemId == c.SetItemId);
                    return snapshot is not null
                        ? (c.QuantityDamaged + c.QuantityMissing) * snapshot.DepositValuePerUnit
                        : 0m;
                });

            decimal depositTotal = booking.BookingItems
                .SelectMany(bi => bi.Components ?? [])
                .Sum(c => c.Quantity * c.DepositValuePerUnit);

            decimal refundAmount = Math.Max(0, depositTotal - totalDeduction);
            booking.DepositAmountKept = totalDeduction;

            await _context.SaveChangesAsync();

            bool hasMissingItems = request.Items
                .SelectMany(i => i.Components)
                .Any(c => c.QuantityMissing > 0);

            if (hasMissingItems)
            {
                var evt = new ReturnAssessedWithMissingItemsEvent(booking.OrderId);
                await _publisher.PublishAsync("inventory.ReturnAssessedWithMissingItems", JsonSerializer.Serialize(evt));
            }

            return NoContent();
        }

        [HttpPut("{id:guid}/missing-items/{assessmentId:guid}/returned")]
        public async Task<IActionResult> MarkMissingItemReturned(Guid id, Guid assessmentId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.ReturnAssessments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null) return NotFound();

            var assessment = booking.BookingItems!
                .SelectMany(bi => bi.ReturnAssessments ?? [])
                .FirstOrDefault(ra => ra.Id == assessmentId);

            if (assessment is null) return NotFound();
            if (assessment.QuantityMissing <= 0) return BadRequest("No missing items on this assessment.");
            if (assessment.QuantityCustomerReturned >= assessment.QuantityMissing)
                return BadRequest("All missing items for this component have already been returned.");

            assessment.QuantityCustomerReturned++;

            var spareStock = await _context.SpareStocks.FirstOrDefaultAsync(ss => ss.SetItemId == assessment.SetItemId);
            if (spareStock != null)
                spareStock.QuantityAvailable++;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost("/api/maintenance/{productSetId:guid}/components/{setItemId:guid}/replaced")]
        public async Task<IActionResult> MarkComponentReplaced(Guid productSetId, Guid setItemId)
        {
            var bookingItems = await _context.BookingItems
                .Include(bi => bi.ReturnAssessments)
                .Where(bi => bi.ProductSetId == productSetId)
                .ToListAsync();

            var assessments = bookingItems
                .SelectMany(bi => bi.ReturnAssessments ?? [])
                .Where(ra => ra.SetItemId == setItemId && ra.ReplacedAt is null
                    && (ra.QuantityDamaged + ra.QuantityMissing) > 0)
                .ToList();

            if (!assessments.Any())
                return BadRequest("No pending replacements for this component.");

            int totalNeeded = assessments.Sum(ra => ra.QuantityDamaged + ra.QuantityMissing);

            var spareStock = await _context.SpareStocks
                .FirstOrDefaultAsync(ss => ss.SetItemId == setItemId);

            if (spareStock is null || spareStock.QuantityAvailable < totalNeeded)
                return BadRequest($"Insufficient spare stock. Need {totalNeeded}, have {spareStock?.QuantityAvailable ?? 0}.");

            spareStock.QuantityAvailable -= totalNeeded;
            foreach (var assessment in assessments)
                assessment.ReplacedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteBookingRequest request)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.ReturnAssessments)
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.Components!)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null) return NotFound();
            if (booking.Status != BookingStatus.Returned)
                return BadRequest("Booking must be Returned before completing.");

            var assessments = booking.BookingItems!
                .SelectMany(bi => bi.ReturnAssessments ?? [])
                .ToList();

            decimal depositTotal = booking.BookingItems!
                .SelectMany(bi => bi.Components ?? [])
                .Sum(c => c.Quantity * c.DepositValuePerUnit);

            decimal totalDeduction = assessments.Sum(ra =>
            {
                var snapshot = booking.BookingItems!
                    .SelectMany(bi => bi.Components ?? [])
                    .FirstOrDefault(c => c.SetItemId == ra.SetItemId);
                return snapshot is not null
                    ? (ra.QuantityDamaged + Math.Max(0, ra.QuantityMissing - ra.QuantityCustomerReturned)) * snapshot.DepositValuePerUnit
                    : 0m;
            });

            decimal refundAmount = Math.Max(0, depositTotal - totalDeduction);

            booking.DepositAmountKept = totalDeduction;
            booking.CompletionNotes = request.CompletionNotes;
            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var evt = new BookingCompletedEvent(
                booking.OrderId,
                booking.Id,
                totalDeduction,
                refundAmount,
                request.CompletionNotes,
                request.ReturnPhotoUrls ?? []);
            await _publisher.PublishAsync("inventory.BookingCompleted", JsonSerializer.Serialize(evt));

            return NoContent();
        }

        [HttpPost("/api/maintenance/{productSetId:guid}/release")]
        public async Task<IActionResult> ReleaseSet(Guid productSetId)
        {
            var productSet = await _context.ProductSets
                .Include(ps => ps.BookingItems!)
                    .ThenInclude(bi => bi.Booking)
                .Include(ps => ps.BookingItems!)
                    .ThenInclude(bi => bi.ReturnAssessments)
                .FirstOrDefaultAsync(ps => ps.Id == productSetId);

            if (productSet is null) return NotFound();
            if (productSet.Status != Status.Maintenance)
                return BadRequest("Set is not in maintenance.");

            if (productSet.CleanedAt is null)
                return BadRequest("Set must be marked as cleaned before releasing.");

            var mostRecentBookingItem = productSet.BookingItems?
                .Where(bi => bi.Booking != null)
                .OrderByDescending(bi => bi.Booking!.ReturnedAt)
                .FirstOrDefault();

            var unreplaced = productSet.BookingItems!
                .Where(bi => bi.BookingId == mostRecentBookingItem?.BookingId)
                .SelectMany(bi => bi.ReturnAssessments ?? [])
                .Where(ra => ra.ReplacedAt is null && (ra.QuantityDamaged + ra.QuantityMissing) > 0)
                .ToList();

            if (unreplaced.Any())
                return BadRequest("All components must be marked as replaced before releasing.");

            productSet.Status = Status.Available;
            productSet.CleanedAt = null;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id:guid}/maintenance")]
        public async Task<IActionResult> GetMaintenanceDetail(Guid id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.Components!)
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.ReturnAssessments!)
                        .ThenInclude(ra => ra.SetItem!)
                            .ThenInclude(si => si.SpareStock)
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.ProductSet)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null) return NotFound();

            var productSetIds = booking.BookingItems!
                .Select(bi => bi.ProductSetId)
                .ToList();

            var sevenDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var upcomingBookings = await _context.BookingItems
                .Where(bi => productSetIds.Contains(bi.ProductSetId)
                    && bi.ReservationDate <= sevenDaysFromNow
                    && bi.ReservationDate > today
                    && bi.Booking!.Status != BookingStatus.Cancelled
                    && bi.BookingId != id)
                .Select(bi => new { bi.ReservationDate, bi.ProductSetId })
                .AsNoTracking()
                .ToListAsync();

            var components = booking.BookingItems!
                .SelectMany(bi => bi.ReturnAssessments ?? [])
                .Where(ra => ra.QuantityDamaged + ra.QuantityMissing > 0)
                .GroupBy(ra => ra.SetItemId)
                .Select(g =>
                {
                    var first = g.First();
                    var snapshot = booking.BookingItems!
                        .SelectMany(bi => bi.Components ?? [])
                        .FirstOrDefault(c => c.SetItemId == g.Key);
                    return new
                    {
                        SetItemId = g.Key,
                        SetItemName = snapshot?.Name ?? first.SetItem?.Name,
                        TotalDamaged = g.Sum(ra => ra.QuantityDamaged),
                        TotalMissing = g.Sum(ra => ra.QuantityMissing),
                        SpareStockAvailable = first.SetItem?.SpareStock?.QuantityAvailable ?? 0,
                        IsReplaced = g.All(ra => ra.ReplacedAt != null)
                    };
                })
                .ToList();

            return Ok(new
            {
                BookingId = id,
                booking.Status,
                Components = components,
                UpcomingBookings = upcomingBookings.Select(ub => new
                {
                    ub.ReservationDate,
                    DaysUntil = (ub.ReservationDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days
                })
            });
        }

        [HttpPost("/api/maintenance/{productSetId:guid}/mark-cleaned")]
        public async Task<IActionResult> MarkCleaned(Guid productSetId)
        {
            var productSet = await _context.ProductSets.FindAsync(productSetId);
            if (productSet is null) return NotFound();

            productSet.CleanedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("/api/maintenance")]
        public async Task<IActionResult> GetMaintenanceQueue()
        {
            var maintenanceSets = await _context.ProductSets
                .Where(ps => ps.Status == Status.Maintenance)
                .Include(ps => ps.Product)
                .Include(ps => ps.BookingItems!)
                    .ThenInclude(bi => bi.Booking)
                .Include(ps => ps.BookingItems!)
                    .ThenInclude(bi => bi.Components!)
                .Include(ps => ps.BookingItems!)
                    .ThenInclude(bi => bi.ReturnAssessments!)
                        .ThenInclude(ra => ra.SetItem!)
                            .ThenInclude(si => si.SpareStock)
                .AsNoTracking()
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var sevenDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
            var setIds = maintenanceSets.Select(ps => ps.Id).ToList();

            var upcomingItems = await _context.BookingItems
                .Where(bi => setIds.Contains(bi.ProductSetId)
                    && bi.ReservationDate > today
                    && bi.ReservationDate <= sevenDaysFromNow
                    && bi.Booking!.Status == BookingStatus.Confirmed)
                .Select(bi => new { bi.ProductSetId, bi.ReservationDate })
                .AsNoTracking()
                .ToListAsync();

            var results = maintenanceSets.Select(ps =>
            {
                var returnedBookingItem = ps.BookingItems?
                    .Where(bi => bi.Booking != null)
                    .OrderByDescending(bi => bi.Booking!.ReturnedAt)
                    .FirstOrDefault();

                var booking = returnedBookingItem?.Booking;

                var allAssessments = ps.BookingItems?
                    .Where(bi => bi.Booking != null)
                    .SelectMany(bi => bi.ReturnAssessments ?? [])
                    .ToList() ?? [];

                var allComponents = ps.BookingItems?
                    .SelectMany(bi => bi.Components ?? [])
                    .ToList() ?? [];

                var upcoming = upcomingItems
                    .Where(u => u.ProductSetId == ps.Id)
                    .Select(u => new
                    {
                        u.ReservationDate,
                        DaysUntil = (u.ReservationDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days
                    })
                    .OrderBy(u => u.DaysUntil)
                    .ToList();

                var components = allAssessments
                    .Where(ra => ra.QuantityDamaged + ra.QuantityMissing > 0 && ra.ReplacedAt == null)
                    .GroupBy(ra => ra.SetItemId)
                    .Select(g =>
                    {
                        var first = g.First();
                        var snapshot = allComponents.FirstOrDefault(c => c.SetItemId == g.Key);
                        return new
                        {
                            SetItemId = g.Key,
                            SetItemName = snapshot?.Name ?? first.SetItem?.Name,
                            TotalDamaged = g.Sum(ra => ra.QuantityDamaged),
                            TotalMissing = g.Sum(ra => ra.QuantityMissing),
                            SpareStockAvailable = first.SetItem?.SpareStock?.QuantityAvailable ?? 0,
                        };
                    })
                    .ToList();

                return new
                {
                    ProductSetId = ps.Id,
                    ProductSetName = ps.Name,
                    ProductId = ps.ProductId,
                    ProductName = ps.Product?.Name,
                    BookingId = booking?.Id,
                    ReturnedAt = booking?.ReturnedAt,
                    IsCleaned = ps.CleanedAt != null,
                    Components = components,
                    UpcomingBookings = upcoming,
                    NextBookingDays = upcoming.Any() ? (int?)upcoming.Min(u => u.DaysUntil) : null
                };
            })
            .OrderBy(r => r.NextBookingDays ?? int.MaxValue)
            .ToList();

            return Ok(results);
        }

        [HttpGet("/api/spare-stock")]
        public async Task<IActionResult> GetAllSpareStock()
        {
            var stock = await _context.SpareStocks
                .Include(ss => ss.SetItem!)
                    .ThenInclude(si => si.Product)
                .Select(ss => new
                {
                    ss.Id,
                    ss.SetItemId,
                    SetItemName = ss.SetItem!.Name,
                    ss.SetItem.ProductId,
                    ProductName = ss.SetItem.Product!.Name,
                    ss.QuantityAvailable
                })
                .AsNoTracking()
                .OrderBy(ss => ss.ProductName)
                .ThenBy(ss => ss.SetItemName)
                .ToListAsync();

            return Ok(stock);
        }

        [HttpPost("spare-stock/{setItemId:guid}/remove")]
        public async Task<IActionResult> RemoveSpareStock(Guid setItemId, [FromBody] AddSpareStockRequest request)
        {
            if (request.Quantity <= 0) return BadRequest("Quantity must be positive.");

            var spareStock = await _context.SpareStocks
                .FirstOrDefaultAsync(ss => ss.SetItemId == setItemId);

            if (spareStock is null) return NotFound();

            spareStock.QuantityAvailable = Math.Max(0, spareStock.QuantityAvailable - request.Quantity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("spare-stock/{setItemId:guid}/add")]
        public async Task<IActionResult> AddSpareStock(Guid setItemId, [FromBody] AddSpareStockRequest request)
        {
            if (request.Quantity <= 0) return BadRequest("Quantity must be positive.");

            var spareStock = await _context.SpareStocks
                .FirstOrDefaultAsync(ss => ss.SetItemId == setItemId);

            if (spareStock is null)
            {
                var setItemExists = await _context.SetItems.AnyAsync(si => si.Id == setItemId);
                if (!setItemExists) return NotFound();

                _context.SpareStocks.Add(new SpareStock
                {
                    SetItemId = setItemId,
                    QuantityAvailable = request.Quantity
                });
            }
            else
            {
                spareStock.QuantityAvailable += request.Quantity;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _context.Bookings
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
                    .ThenInclude(bi => bi.ProductSet)
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.Components!)
                .Include(b => b.BookingItems!)
                    .ThenInclude(bi => bi.ReturnAssessments!)
                        .ThenInclude(ra => ra.SetItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null)
                return NotFound();

            var setItemsByBookingItem = booking.BookingItems!
                .ToDictionary(
                    bi => bi.Id,
                    bi => (bi.Components ?? []).Select(c => new { Id = c.SetItemId, c.Name, c.Quantity, c.DepositValuePerUnit }).ToList()
                );

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
                booking.DepositAmountKept,
                SetItemsByBookingItem = setItemsByBookingItem,
                BookingItems = booking.BookingItems!.Select(bi => new
                {
                    bi.Id,
                    bi.ReservationDate,
                    bi.ReturnNotes,
                    bi.ReturnedAt,
                    bi.CompletedAt,
                    ProductId = bi.ProductId,
                    ProductName = bi.Product!.Name,
                    ProductColour = bi.Product.Colour,
                    ProductSetId = bi.ProductSetId,
                    ProductSetName = bi.ProductSet?.Name,
                    Components = bi.Components?.Select(c => new
                    {
                        c.SetItemId,
                        c.Name,
                        c.Quantity,
                        c.DepositValuePerUnit
                    }),
                    ReturnAssessments = bi.ReturnAssessments?.Select(ra => new
                    {
                        ra.Id,
                        ra.SetItemId,
                        SetItemName = ra.SetItem?.Name,
                        ra.QuantityGood,
                        ra.QuantityDamaged,
                        ra.QuantityMissing,
                        ra.QuantityCustomerReturned,
                        ra.ReplacedAt
                    })
                })
            };

            return Ok(result);
        }
    }

    public record SetItemAssessmentDto(Guid SetItemId, int QuantityGood, int QuantityDamaged, int QuantityMissing);
    public record BookingItemAssessmentDto(Guid BookingItemId, List<SetItemAssessmentDto> Components, string? ReturnNotes);
    public record MarkReturnedRequest(List<BookingItemAssessmentDto> Items);
    public record CompleteBookingRequest(string? CompletionNotes, List<string>? ReturnPhotoUrls);
    public record AddSpareStockRequest(int Quantity);
}
