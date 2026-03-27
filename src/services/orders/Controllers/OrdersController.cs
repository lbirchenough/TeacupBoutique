using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orders.Data;
using orders.Entities;
using orders.Interfaces;
using orders.Models;
using orders.Services;

namespace orders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(OrdersDbContext _context, IOrderEvent orderEventService, TurnstileService turnstileService) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!await turnstileService.VerifyAsync(dto.TurnstileToken))
                return BadRequest("CAPTCHA verification failed.");
            var order = new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}",
                UserId = dto.UserId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                Subtotal = dto.Subtotal,
                Tax = dto.Tax,
                Total = dto.Total,
                DepositTotal = dto.DepositTotal,
                PickupDate = dto.PickupDate,
                ReturnDate = dto.ReturnDate,
                ReservationDate = dto.ReservationDate,
                // EventType = dto.EventType,
                // GuestCount = dto.GuestCount,
                // SpecialRequests = dto.SpecialRequests,
                OrderItems = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    ReservationDate = i.RentalDate,
                    Name = i.ProductName,
                    Colour = i.ProductThemeColor,
                    ImageUrl = i.ProductImageUrl,
                    UnitPrice = i.PricePerDay,
                    Total = i.Subtotal,
                    DepositAmount = i.DepositAmount
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await orderEventService.PublishOrder(order);

            return CreatedAtAction(nameof(GetOrder), new { orderNumber = order.OrderNumber }, order);
        }

        [HttpGet("{orderNumber}")]
        public async Task<IActionResult> GetOrder(string orderNumber, [FromQuery] Guid? token)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order is null) return NotFound();

            var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault();
            var isOwner = Guid.TryParse(userIdStr, out var userId) && order.UserId == userId;
            var hasValidToken = token.HasValue && order.AccessToken == token.Value;

            if (!isOwner && !hasValidToken) return StatusCode(403);

            return Ok(order);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault();
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("admin/{orderId:guid}")]
        public async Task<IActionResult> GetOrderByIdForAdmin(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null) return NotFound();
            return Ok(order);
        }

        [HttpPost("{orderNumber}/cancel")]
        public async Task<IActionResult> CancelOrder(string orderNumber, [FromQuery] Guid? token)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            if (order is null) return NotFound();

            var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault();
            var isOwner = Guid.TryParse(userIdStr, out var userId) && order.UserId == userId;
            var hasValidToken = token.HasValue && order.AccessToken == token.Value;
            if (!isOwner && !hasValidToken) return StatusCode(403);

            if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.OutOfStock)
                return BadRequest($"Order cannot be cancelled in its current state ({order.Status}).");

            await orderEventService.CancelOrder(order, "Cancelled by customer");
            return NoContent();
        }

        [HttpPatch("claim")]
        public async Task<IActionResult> ClaimOrders()
        {
            var email = Request.Headers["X-User-Email"].FirstOrDefault();
            var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault();

            if (string.IsNullOrEmpty(email) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.CustomerEmail == email && o.UserId == null)
                .ToListAsync();

            foreach (var order in orders)
                order.UserId = userId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // [HttpPut("{id:guid}")]
        // public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] CreateOrderDto dto)
        // {
        //     var order = await _context.Orders
        //         .Include(o => o.OrderItems)
        //         .FirstOrDefaultAsync(o => o.Id == id);
        //     if (order is null)
        //         return NotFound();

        //     // Only allow updating certain fields for simplicity
        //     order.CustomerName = dto.CustomerName;
        //     order.CustomerEmail = dto.CustomerEmail;
        //     order.CustomerPhone = dto.CustomerPhone;
        //     order.PickupDate = dto.PickupDate;
        //     order.ReturnDate = dto.ReturnDate;
        //     order.ReservationDate = dto.ReservationDate;
        //     // order.EventType = dto.EventType;
        //     // order.GuestCount = dto.GuestCount;
        //     // order.SpecialRequests = dto.SpecialRequests;
        //     order.UpdatedAt = DateTime.UtcNow;
        //     order.OrderItems = dto.Items.Select(i => new OrderItem
        //     {
        //         ProductId = i.ProductId,
        //         Quantity = i.Quantity,
        //         ReservationDate = i.RentalDate,
        //         Name = i.ProductName,
        //         Colour = i.ProductThemeColor,
        //         ImageUrl = i.ProductImageUrl,
        //         UnitPrice = i.PricePerDay,
        //         Total = i.Subtotal
        //     }).ToList();

        //     await _context.SaveChangesAsync();
        //     return NoContent();
        // }

    
    }
}
