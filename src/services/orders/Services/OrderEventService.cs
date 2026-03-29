using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using orders.Data;
using orders.Entities;
using orders.Interfaces;
using orders.Models;

namespace orders.Services;

public class OrderEventService(IMessagePublisher _publisher, OrdersDbContext _context, IConfiguration _configuration) : IOrderEvent
{
    public async Task PublishOrder(Order order)
    {
        var dto = new OrderPlacedDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            ReservationDate = order.PickupDate,
            PlacedAt = order.CreatedAt,
            Items = order.OrderItems?.Select(oi => new OrderPlacedItemDto
            {
                ProductId = oi.ProductId,
                Name = oi.Name,
                Quantity = oi.Quantity,
            }).ToList() ?? []
        };

        await _publisher.PublishAsync("orders.OrderPlaced", JsonSerializer.Serialize(dto));
    }

    public async Task HandleStockReserved(string message)
    {
        var payload = JsonSerializer.Deserialize<StockReservedDto>(message);
        if (payload is null) return;

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == payload.OrderId);
        if (order is null) { Console.WriteLine($" [orders] Order {payload.OrderId} not found"); return; }

        var taxRate = _configuration.GetValue<decimal>("TaxRate", 0.10m);
        var rentalTotal = payload.Items.Sum(i => i.UnitPrice * i.Quantity);
        var depositTotal = payload.Items.Sum(i => i.DepositAmount * i.Quantity);
        var subtotal = Math.Round(rentalTotal / (1 + taxRate), 2);
        var tax = Math.Round(rentalTotal - subtotal, 2);
        var total = rentalTotal + depositTotal;

        order.Subtotal = subtotal;
        order.Tax = tax;
        order.Total = total;
        order.DepositTotal = depositTotal;
        order.Status = OrderStatus.AwaitingPayment;

        if (order.OrderItems != null)
        {
            foreach (var item in order.OrderItems)
            {
                var priceItem = payload.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (priceItem is not null)
                {
                    item.UnitPrice = priceItem.UnitPrice;
                    item.Total = priceItem.UnitPrice * item.Quantity;
                    item.DepositAmount = priceItem.DepositAmount * item.Quantity;
                }
            }
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($" [orders] Order {payload.OrderId} prices stamped, status → AwaitingPayment");

        await _publisher.PublishAsync("orders.ReadyForPayment", JsonSerializer.Serialize(new ReadyForPaymentDto(payload.OrderId, total)));
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
            await CancelOrderAsync(order, "Payment failed after 3 attempts");
            Console.WriteLine($" [orders] Order {payload.OrderId} cancelled after 3 failed payment attempts");
        }
        else
        {
            await _context.SaveChangesAsync();
            Console.WriteLine($" [orders] Order {payload.OrderId} payment attempt {order.PaymentAttempts} of 3");
        }
    }

    public async Task CancelOrder(Order order, string reason)
    {
        await CancelOrderAsync(order, reason);
        Console.WriteLine($" [orders] Order {order.Id} cancelled: {reason}");
    }

    public async Task HandleBookingCancelled(string message)
    {
        var payload = JsonSerializer.Deserialize<BookingCancelledEvent>(message);
        if (payload is null) return;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payload.OrderId);
        if (order is null) { Console.WriteLine($" [orders] Order {payload.OrderId} not found for booking cancellation"); return; }

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Completed)
        {
            Console.WriteLine($" [orders] Order {payload.OrderId} already in terminal state, skipping");
            return;
        }

        await CancelOrderAsync(order, "Booking cancelled by administrator");
        Console.WriteLine($" [orders] Order {payload.OrderId} cancelled via booking {payload.BookingId}");
    }

    public async Task HandleBookingCompleted(string message)
    {
        var payload = JsonSerializer.Deserialize<BookingCompletedEvent>(message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload is null) return;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payload.OrderId);
        if (order is null) { Console.WriteLine($" [orders] Order {payload.OrderId} not found for booking completion"); return; }

        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
        {
            Console.WriteLine($" [orders] Order {payload.OrderId} already in terminal state, skipping completion");
            return;
        }

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        order.DepositAmountKept = payload.DepositAmountKept;
        order.CompletionNotes = payload.CompletionNotes;
        order.ReturnPhotoUrls = payload.ReturnPhotoUrls.Count > 0
            ? JsonSerializer.Serialize(payload.ReturnPhotoUrls)
            : null;
        await _context.SaveChangesAsync();

        var completedDto = new OrderCompletedDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            AccessToken = order.AccessToken,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            ReservationDate = order.ReservationDate,
            DepositAmountKept = payload.DepositAmountKept,
            CompletionNotes = payload.CompletionNotes
        };
        await _publisher.PublishAsync("orders.OrderCompleted", JsonSerializer.Serialize(completedDto));
        Console.WriteLine($" [orders] Order {payload.OrderId} completed via booking {payload.BookingId}");
    }

    private async Task CancelOrderAsync(Order order, string reason)
    {
        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;
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
    }
}