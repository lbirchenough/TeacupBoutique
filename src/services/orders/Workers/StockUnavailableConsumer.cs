using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class StockUnavailableConsumer(IServiceScopeFactory scopeFactory, ILogger<StockUnavailableConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.stock-unavailable";
    public string RoutingKey => "inventory.StockUnavailable";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("StockUnavailable received for orderId: {Message}", message);
        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleStockUnavailable(message);
    }
}
