using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class StockUnavailableConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "orders.stock-unavailable";
    public override string RoutingKey => "inventory.StockUnavailable";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("StockUnavailable received for orderId: {Message}", message);
        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleStockUnavailable(message);
    }
}
