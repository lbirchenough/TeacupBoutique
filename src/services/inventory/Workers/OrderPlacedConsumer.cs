using Messaging.Workers;
using System.Text.Json;
using inventory.Services;
using orders.Models;

namespace inventory.Workers;

public class OrderPlacedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "inventory.order-placed";
    public override string RoutingKey => "orders.OrderPlaced";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("OrderPlaced received: {Message}", message);

        var order = JsonSerializer.Deserialize<OrderPlacedDto>(message);
        if (order is null) return;

        using var scope = scopeFactory.CreateScope();
        var inventoryEventService = scope.ServiceProvider.GetRequiredService<InventoryEventService>();
        await inventoryEventService.CheckStockAndPublishEvent(order);
    }
}
