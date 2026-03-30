using Messaging.Workers;
using System.Text.Json;
using inventory.Services;
using orders.Models;

namespace inventory.Workers;

public class OrderPlacedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    public override string QueueName => "inventory.order-placed";
    public override string RoutingKey => "orders.OrderPlaced";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [inventory] OrderPlaced received: {message}");

        var order = JsonSerializer.Deserialize<OrderPlacedDto>(message);
        if (order is null) return;

        using var scope = scopeFactory.CreateScope();
        var inventoryEventService = scope.ServiceProvider.GetRequiredService<InventoryEventService>();
        await inventoryEventService.CheckStockAndPublishEvent(order);
    }
}
