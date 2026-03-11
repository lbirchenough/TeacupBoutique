using System;
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
        await _publisher.PublishAsync("orders.OrderPlaced", json);
    }


    public async Task HandleStockReserved(string orderId)
    {
        
        var id = Guid.Parse(orderId);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
       
        if (order != null)
        {
            order.Status = OrderStatus.AwaitingPayment;
            await _context.SaveChangesAsync();
            Console.WriteLine($" [orders] Handled StockReserved for orderId: {orderId}, updated order status to AwaitingPayment");
        }
        else
        {
            Console.WriteLine($" [orders] Order with id {orderId} not found");
        }
        
    }

    public async Task HandleStockUnavailable(string orderId)
    {
        var id = Guid.Parse(orderId);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        
        if (order != null)
        {
            order.Status = OrderStatus.OutOfStock;
            await _context.SaveChangesAsync();
            Console.WriteLine($" [orders] Handled StockUnavailable for orderId: {orderId}, updated order status to Cancelled");
        }
        else
        {
            Console.WriteLine($" [orders] Order with id {orderId} not found");
        }
    }

    public async Task HandlePaymentCaptured(string message)
    {
        var payload = JsonSerializer.Deserialize<PaymentEventDto>(message);
        if (payload is null) return;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payload.OrderId);

        if (order is null)
        {
            Console.WriteLine($" [orders] Order {payload.OrderId} not found");
            return;
        }

        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        Console.WriteLine($" [orders] Order {payload.OrderId} confirmed after payment captured");
    }

    public async Task HandlePaymentFailed(string message)
    {
        var payload = JsonSerializer.Deserialize<PaymentEventDto>(message);
        if (payload is null) return;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payload.OrderId);
        if (order is null)
        {
            Console.WriteLine($" [orders] Order {payload.OrderId} not found");
            return;
        }

        order.PaymentAttempts++;
        Console.WriteLine($" [orders] Payment failed for order {payload.OrderId}, attempt {order.PaymentAttempts}");

        if (order.PaymentAttempts >= 3)
        {
            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            order.CancellationReason = "Payment failed after 3 attempts";
            await _context.SaveChangesAsync();

            var cancelPayload = JsonSerializer.Serialize(new { OrderId = order.Id });
            await _publisher.PublishAsync("orders.OrderCancelled", cancelPayload);
            Console.WriteLine($" [orders] Order {payload.OrderId} cancelled after 3 failed payment attempts");
        }
        else
        {
            await _context.SaveChangesAsync();
            Console.WriteLine($" [orders] Order {payload.OrderId} still awaiting payment ({3 - order.PaymentAttempts} attempt(s) remaining)");
        }
    }
}