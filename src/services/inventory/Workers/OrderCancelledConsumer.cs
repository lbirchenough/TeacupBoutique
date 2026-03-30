using Messaging.Workers;
using System.Text.Json;
using inventory.Services;

namespace inventory.Workers;

public class OrderCancelledConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    public override string QueueName => "inventory.order-cancelled";
    public override string RoutingKey => "orders.OrderCancelled";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [inventory] OrderCancelled received: {message}");

        var payload = JsonSerializer.Deserialize<OrderCancelledPayload>(message);
        if (payload is null) return;

        using var scope = scopeFactory.CreateScope();
        var inventoryEventService = scope.ServiceProvider.GetRequiredService<InventoryEventService>();
        await inventoryEventService.HandleOrderCancelled(payload.OrderId);
    }

    private record OrderCancelledPayload(Guid OrderId);
}
