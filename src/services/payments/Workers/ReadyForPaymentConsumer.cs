using Messaging.Workers;
using System.Text.Json;
using payments.Data;
using payments.Entities;

namespace payments.Workers;

public class ReadyForPaymentConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "payments.ready-for-payment";
    public override string RoutingKey => "orders.ReadyForPayment";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<ReadyForPaymentDto>(message);
        if (payload is null) return;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        var existing = await db.PendingPaymentAmounts.FindAsync(payload.OrderId);
        if (existing is not null) return; // idempotent

        db.PendingPaymentAmounts.Add(new PendingPaymentAmount
        {
            OrderId = payload.OrderId,
            Amount = payload.Amount,
        });
        await db.SaveChangesAsync();
        _logger.LogDebug("Stored pending amount {Amount} for order {OrderId}", payload.Amount, payload.OrderId);
    }
}

record ReadyForPaymentDto(Guid OrderId, decimal Amount);
