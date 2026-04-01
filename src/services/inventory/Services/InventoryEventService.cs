using System.Text.Json;
using inventory.Data;
using inventory.Entities;
using Messaging.Interfaces;
using inventory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using orders.Models;

namespace inventory.Services;

public class InventoryEventService(IMessagePublisher _publisher, InventoryDbContext _context, ILogger<InventoryEventService> _logger)
{
    public async Task CheckStockAndPublishEvent(OrderPlacedDto placedOrder)
    {
        var productIds = placedOrder.Items.Select(i => i.ProductId).ToList();
        bool allInStock = false;
        StockReservedDto? stockReserved = null;

        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var bookingItems = new List<BookingItem>();
            allInStock = true;

            foreach (var item in placedOrder.Items)
            {
                var availableSets = await _context.ProductSets
                    .Where(ps => ps.ProductId == item.ProductId
                        && ps.Status != Status.Retired
                        && !ps.BookingItems!.Any(bi =>
                            bi.ReservationDate == placedOrder.ReservationDate
                            && bi.Booking!.Status != BookingStatus.Cancelled))
                    .Take(item.Quantity)
                    .ToListAsync();

                if (availableSets.Count < item.Quantity)
                {
                    allInStock = false;
                    break;
                }

                foreach (var productSet in availableSets)
                {
                    bookingItems.Add(new BookingItem
                    {
                        ProductId = item.ProductId,
                        ProductSetId = productSet.Id,
                        ReservationDate = placedOrder.ReservationDate,
                    });
                }
            }

            if (allInStock)
            {
                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    OrderId = placedOrder.OrderId,
                    ReservationDate = placedOrder.ReservationDate,
                    Status = BookingStatus.Reserved,
                    ReservedAt = DateTime.UtcNow,
                    BookingItems = bookingItems
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                stockReserved = new StockReservedDto
                {
                    OrderId = placedOrder.OrderId,
                    Items = placedOrder.Items.Select(i => new StockReservedItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = products[i.ProductId].Price,
                        DepositAmount = products[i.ProductId].DepositAmount,
                    }).ToList()
                };
            }
            else
            {
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error reserving stock for order {OrderId}", placedOrder.OrderId);
            allInStock = false;
        }

        // Publish events after transaction is committed/rolled back
        if (allInStock && stockReserved is not null)
            await _publisher.PublishAsync("inventory.StockReserved", JsonSerializer.Serialize(stockReserved));
        else
            await _publisher.PublishAsync("inventory.StockUnavailable", placedOrder.OrderId.ToString());
    }

    public async Task HandlePaymentSucceeded(Guid orderId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.OrderId == orderId);
        if (booking is null)
        {
            _logger.LogWarning("No booking found for payment captured on order {OrderId}", orderId);
            return;
        }

        if (booking.Status != BookingStatus.Reserved)
        {
            _logger.LogWarning("Booking {BookingId} is not in Reserved status, skipping confirmation", booking.Id);
            return;
        }

        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} confirmed for order {OrderId}", booking.Id, orderId);
    }

    public async Task HandleOrderCancelled(Guid orderId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.OrderId == orderId);
        if (booking is null)
        {
            _logger.LogWarning("No booking found for cancelled order {OrderId}", orderId);
            return;
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Booking cancelled and stock released for order {OrderId}", orderId);
    }
}
