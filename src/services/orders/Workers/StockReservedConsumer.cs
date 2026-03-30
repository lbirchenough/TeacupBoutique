using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class StockReservedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    public override string QueueName => "orders.stock-reserved";
    public override string RoutingKey => "inventory.StockReserved";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] StockReserved received for orderId: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleStockReserved(message);

    }
}
