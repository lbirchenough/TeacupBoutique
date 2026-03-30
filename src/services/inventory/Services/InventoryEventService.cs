using System;
using System.Text.Json;
using inventory.Data;
using inventory.Entities;
using Messaging.Interfaces;
using inventory.Models;
using Microsoft.EntityFrameworkCore;
using orders.Models;

namespace inventory.Services;

public class InventoryEventService(IMessagePublisher _publisher, InventoryDbContext _context)
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
                var availableItems = await _context.InventoryItems
                    .Where(inv => inv.ProductId == item.ProductId
                        && inv.Status == Status.Available
                        && !inv.BookingItems.Any(bi =>
                            bi.ReservationDate == placedOrder.ReservationDate
                            && bi.Booking!.Status != BookingStatus.Cancelled))
                    .Take(item.Quantity)
                    .ToListAsync();

                if (availableItems.Count < item.Quantity)
                {
                    allInStock = false;
                    break;
                }

                foreach (var inventoryItem in availableItems)
                {
                    bookingItems.Add(new BookingItem
                    {
                        ProductId = item.ProductId,
                        InventoryItemId = inventoryItem.Id,
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
            Console.WriteLine($" [inventory] Error reserving stock for order {placedOrder.OrderId}: {ex.Message}");
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
            Console.WriteLine($" [inventory] No booking found for payment captured on order {orderId}");
            return;
        }

        if (booking.Status != BookingStatus.Reserved)
        {
            Console.WriteLine($" [inventory] Booking {booking.Id} is not in Reserved status, skipping confirmation");
            return;
        }

        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        Console.WriteLine($" [inventory] Booking {booking.Id} confirmed for order {orderId}");
    }

    public async Task HandleOrderCancelled(Guid orderId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.OrderId == orderId);
        if (booking is null)
        {
            Console.WriteLine($" [inventory] No booking found for cancelled order {orderId}");
            return;
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        Console.WriteLine($" [inventory] Booking cancelled and stock released for order {orderId}");
    }
}
