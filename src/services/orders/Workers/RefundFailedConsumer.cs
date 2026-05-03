using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class RefundFailedConsumer(IServiceScopeFactory scopeFactory, ILogger<RefundFailedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.refund-failed";
    public string RoutingKey => "payments.RefundFailed";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("RefundFailed received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleRefundFailed(message);
    }
}
