using orders.Interfaces;

namespace orders.Workers;

public class StockReservedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "orders.stock-reserved";
    protected override string RoutingKey => "inventory.StockReserved";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] StockReserved received for orderId: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleStockReserved(message);

    }
}
