using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class ReturnAssessedWithMissingItemsConsumer(IServiceScopeFactory scopeFactory, ILogger<ReturnAssessedWithMissingItemsConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.return-assessed-missing";
    public string RoutingKey => "inventory.ReturnAssessedWithMissingItems";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("ReturnAssessedWithMissingItems received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleReturnAssessedWithMissingItems(message);
    }
}
