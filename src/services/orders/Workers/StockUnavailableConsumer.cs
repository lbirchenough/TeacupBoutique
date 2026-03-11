using orders.Interfaces;

namespace orders.Workers;

public class StockUnavailableConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "orders.stock-unavailable";
    protected override string RoutingKey => "inventory.StockUnavailable";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] StockUnavailable received for orderId: {message}");
        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleStockUnavailable(message);
    }
}
