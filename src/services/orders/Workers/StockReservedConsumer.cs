using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class StockReservedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "orders.stock-reserved";
    public override string RoutingKey => "inventory.StockReserved";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("StockReserved received for orderId: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleStockReserved(message);
    }
}
