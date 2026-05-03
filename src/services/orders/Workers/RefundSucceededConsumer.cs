using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class RefundSucceededConsumer(IServiceScopeFactory scopeFactory, ILogger<RefundSucceededConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.refund-succeeded";
    public string RoutingKey => "payments.RefundSucceeded";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("RefundSucceeded received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleRefundSucceeded(message);
    }
}
