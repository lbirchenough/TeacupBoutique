using System;
using System.Text.Json;
using orders.Entities;
using orders.Interfaces;
using orders.Models;

namespace orders.Services;

public class OrderEventService(IMessagePublisher _publisher) : IOrderEvent
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
        await _publisher.PublishAsync("commerce.events.OrderPlaced", json);
    }

    // public async Task PublishOrderCancelled(Order order) { ... }
}