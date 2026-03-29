using System.Text.Json;
using payments.Data;
using payments.Entities;

namespace payments.Workers;

public class ReadyForPaymentConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "payments.ready-for-payment";
    protected override string RoutingKey => "orders.ReadyForPayment";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
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
        Console.WriteLine($" [payments] Stored pending amount {payload.Amount} for order {payload.OrderId}");
    }
}

record ReadyForPaymentDto(Guid OrderId, decimal Amount);
