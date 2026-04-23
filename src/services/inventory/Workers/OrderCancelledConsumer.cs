using Messaging.Interfaces;
using System.Text.Json;
using inventory.Services;

namespace inventory.Workers;

public class OrderCancelledConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderCancelledConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "inventory.order-cancelled";
    public string RoutingKey => "orders.OrderCancelled";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("OrderCancelled received: {Message}", message);

        var payload = JsonSerializer.Deserialize<OrderCancelledPayload>(message);
        if (payload is null) return;

        using var scope = scopeFactory.CreateScope();
        var inventoryEventService = scope.ServiceProvider.GetRequiredService<InventoryEventService>();
        await inventoryEventService.HandleOrderCancelled(payload.OrderId);
    }

    private record OrderCancelledPayload(Guid OrderId);
}
