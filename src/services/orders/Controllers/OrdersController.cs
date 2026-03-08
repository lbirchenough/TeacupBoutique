using System.Text;
using System.Text.Json;
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
            await PublishOrder();
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

            //await PublishOrder(order);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        public async Task PublishOrder(){
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false,
            //     arguments: null);
            await channel.ExchangeDeclareAsync(exchange: "commerce.events", type: ExchangeType.Topic);
            // const string message = "Hello World!";
            // var body = Encoding.UTF8.GetBytes(message);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}",
                CustomerName = "John Doe",
                CustomerEmail = "john.doe@example.com",
                CustomerPhone = "123-456-7890",
                OrderItems = new List<OrderItem>(),
            };

            order.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 2,
                ReservationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                Name = "Vintage Teacup Set",
                Colour = "Floral",
                ImageUrl = "https://example.com/images/teacup-set.jpg",
                UnitPrice = 29.99m,
                Total = 59.98m
            });

            order.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 1,
                ReservationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                Name = "Antique Silver Teapot",
                Colour = "Silver",
                ImageUrl = "https://example.com/images/silver-teapot.jpg",
                UnitPrice = 49.99m,
                Total = 49.99m
            });
            
            var orderPlacedDto = new OrderPlacedDto
            {
                OrderId = order.Id,
                Items = order.OrderItems?.Select(oi => new OrderPlacedItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    ClaimedPricePerDay = oi.UnitPrice
                }).ToList() ?? [],
                ReservationDate = order.PickupDate,
                PlacedAt = order.CreatedAt
            };

            var json = JsonSerializer.Serialize(orderPlacedDto);
            var body = Encoding.UTF8.GetBytes(json);

            //await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            await channel.BasicPublishAsync(exchange: "commerce.events", routingKey: "commerce.events.OrderPlaced", body: body);
            
            Console.WriteLine($" [x] Sent {json}");

        }


    }
}
