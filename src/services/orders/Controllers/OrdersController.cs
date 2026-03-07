using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orders.Data;
using orders.Entities;
using orders.Models;
using RabbitMQ.Client;

namespace orders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(OrdersDbContext _context) : ControllerBase
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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var order = new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}"[..50],
                UserId = dto.UserId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                Subtotal = dto.Subtotal,
                Tax = dto.Tax,
                Total = dto.Total,
                PickupDate = dto.PickupDate,
                ReturnDate = dto.ReturnDate,
                EventDate = dto.EventDate,
                EventType = dto.EventType,
                GuestCount = dto.GuestCount,
                SpecialRequests = dto.SpecialRequests,
                OrderItems = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    RentalDate = i.RentalDate,
                    ProductName = i.ProductName,
                    ProductThemeColor = i.ProductThemeColor,
                    ProductImageUrl = i.ProductImageUrl,
                    PricePerDay = i.PricePerDay,
                    Subtotal = i.Subtotal
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await PublishOrder(order);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        public async Task PublishOrder(Order order){
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false,
                arguments: null);
            const string message = "Hello World!";
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            Console.WriteLine($" [x] Sent {message}");

        }


    }
}
