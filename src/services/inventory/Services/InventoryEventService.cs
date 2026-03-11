using System;
using System.Text.Json;
using inventory.Data;
using inventory.Entities;
using inventory.Interfaces;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using orders.Models;

namespace inventory.Services;

public class InventoryEventService(IMessagePublisher _publisher, InventoryDbContext _context)
{
    public async Task CheckStockAndPublishEvent(OrderPlacedDto placedOrder)
    {
        //Check stock
        //Get product and inventory items, get a count of booking items on specific date, compare against total inventory count , if less then one is available.
        //Repeat for each productId

        //TODO - price validation logic

        bool allInStock = true;

        var bookingItems = new List<BookingItem>();

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
        
        
        
        // foreach (var item in placedOrder.Items)
        // {
        //     int totalProductStock = await _context.InventoryItems
        //         .CountAsync(i => i.ProductId == item.ProductId && i.Status == Status.Available);

            
        //     int currentlyBookedCount = await _context.BookingItems
        //         .Where(bi => bi.ProductId == item.ProductId && bi.ReservationDate == placedOrder.ReservationDate && bi.Booking.Status != BookingStatus.Cancelled)
        //         .CountAsync();

        //     allInStock = currentlyBookedCount + item.Quantity <= totalProductStock;
        // }

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
            
            //var json = JsonSerializer.Serialize(placedOrder.OrderId);
            await _publisher.PublishAsync("inventory.StockReserved", placedOrder.OrderId.ToString());

        }

        else
        {
            // Publish out of stock event or handle accordingly
            await _publisher.PublishAsync("inventory.StockUnavailable", placedOrder.OrderId.ToString());
        }
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
