using Messaging.Interfaces;
using System.Text.Json;
using inventory.Services;
using orders.Models;

namespace inventory.Workers;

public class OrderPlacedConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderPlacedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "inventory.order-placed";
    public string RoutingKey => "orders.OrderPlaced";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("OrderPlaced received: {Message}", message);

        var order = JsonSerializer.Deserialize<OrderPlacedDto>(message);
        if (order is null) return;

        using var scope = scopeFactory.CreateScope();
        var inventoryEventService = scope.ServiceProvider.GetRequiredService<InventoryEventService>();
        await inventoryEventService.CheckStockAndPublishEvent(order);
    }
}
