using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using orders.Data;
using orders.Entities;
using orders.Interfaces;
using orders.Models;

namespace orders.Services;

public class OrderEventService(IMessagePublisher _publisher, OrdersDbContext _context) : IOrderEvent
{
    public async Task PublishOrder(Order order)
    {
        var dto = new OrderPlacedDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            Total = order.Total,
            ReservationDate = order.PickupDate,
            PlacedAt = order.CreatedAt,
            Items = order.OrderItems?.Select(oi => new OrderPlacedItemDto
            {
                ProductId = oi.ProductId,
                Name = oi.Name,
                Quantity = oi.Quantity,
                ClaimedPricePerDay = oi.UnitPrice
            }).ToList() ?? []
        };

        await _publisher.PublishAsync("orders.OrderPlaced", JsonSerializer.Serialize(dto));
    }

    public async Task HandleStockReserved(string orderId)
    {
        var id = Guid.Parse(orderId);
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) { Console.WriteLine($" [orders] Order {orderId} not found"); return; }

        order.Status = OrderStatus.AwaitingPayment;
        await _context.SaveChangesAsync();
        Console.WriteLine($" [orders] Order {orderId} status → AwaitingPayment");
    }

    public async Task HandleStockUnavailable(string orderId)
    {
        var id = Guid.Parse(orderId);
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) { Console.WriteLine($" [orders] Order {orderId} not found"); return; }

        order.Status = OrderStatus.OutOfStock;
        await _context.SaveChangesAsync();
        Console.WriteLine($" [orders] Order {orderId} status → OutOfStock");
    }

    public async Task HandlePaymentSucceeded(string message)
    {
        var payload = JsonSerializer.Deserialize<PaymentEventDto>(message);
        if (payload is null) return;

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == payload.OrderId);

        if (order is null) { Console.WriteLine($" [orders] Order {payload.OrderId} not found"); return; }

        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var confirmed = new OrderConfirmedDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            AccessToken = order.AccessToken,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            Total = order.Total,
            ReservationDate = order.PickupDate,
            Items = order.OrderItems?.Select(i => new OrderConfirmedItemDto
            {
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList() ?? []
        };

        await _publisher.PublishAsync("orders.OrderConfirmed", JsonSerializer.Serialize(confirmed));
        Console.WriteLine($" [orders] Order {payload.OrderId} confirmed");
    }

    public async Task HandlePaymentFailed(string message)
    {
        var payload = JsonSerializer.Deserialize<PaymentEventDto>(message);
        if (payload is null) return;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payload.OrderId);
        if (order is null) { Console.WriteLine($" [orders] Order {payload.OrderId} not found"); return; }

        order.PaymentAttempts++;

        var failedDto = new PaymentFailedNotificationDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            ReservationDate = order.PickupDate,
            Attempt = order.PaymentAttempts
        };

        await _publisher.PublishAsync("orders.PaymentFailed", JsonSerializer.Serialize(failedDto));

        if (order.PaymentAttempts >= 3)
        {
            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            order.CancellationReason = "Payment failed after 3 attempts";
            await _context.SaveChangesAsync();

            var cancelledDto = new OrderCancelledDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                ReservationDate = order.PickupDate
            };

            await _publisher.PublishAsync("orders.OrderCancelled", JsonSerializer.Serialize(cancelledDto));
            Console.WriteLine($" [orders] Order {payload.OrderId} cancelled after 3 failed payment attempts");
        }
        else
        {
            await _context.SaveChangesAsync();
            Console.WriteLine($" [orders] Order {payload.OrderId} payment attempt {order.PaymentAttempts} of 3");
        }
    }
}