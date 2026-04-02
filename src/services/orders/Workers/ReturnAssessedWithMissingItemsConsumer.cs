using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class ReturnAssessedWithMissingItemsConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "orders.return-assessed-missing";
    public override string RoutingKey => "inventory.ReturnAssessedWithMissingItems";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("ReturnAssessedWithMissingItems received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleReturnAssessedWithMissingItems(message);
    }
}
