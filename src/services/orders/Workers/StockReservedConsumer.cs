using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class StockReservedConsumer(IServiceScopeFactory scopeFactory, ILogger<StockReservedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.stock-reserved";
    public string RoutingKey => "inventory.StockReserved";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("StockReserved received for orderId: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleStockReserved(message);
    }
}
