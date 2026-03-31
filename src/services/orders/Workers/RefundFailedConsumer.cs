using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class RefundFailedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "orders.refund-failed";
    public override string RoutingKey => "payments.RefundFailed";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("RefundFailed received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleRefundFailed(message);
    }
}
