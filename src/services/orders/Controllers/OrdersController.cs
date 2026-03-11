using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orders.Data;
using orders.Entities;
using orders.Interfaces;
using orders.Models;
using orders.Services;
using RabbitMQ.Client;

namespace orders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(OrdersDbContext _context, IOrderEvent orderEventService) : ControllerBase
    {
        
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            
            var orders = await _context.Orders
                .Where(x => x.Status != OrderStatus.Completed)
                //.Include(o => o.OrderItems)
                .AsNoTracking()
                .ToListAsync();
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
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
                    Total = i.Subtotal
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await orderEventService.PublishOrder(order);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null)
                return NotFound();
            return Ok(order);
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
