using System.Text.Json;
using Messaging.Interfaces;
using Messaging.Workers;
using Microsoft.EntityFrameworkCore;
using payments.Data;
using payments.Entities;
using Stripe;

namespace payments.Workers;

public class RefundRequestedConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    IMessagePublisher publisher,
    ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "payments.refund-requested";
    public override string RoutingKey => "orders.RefundRequested";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("RefundRequested received: {Message}", message);

        var payload = JsonSerializer.Deserialize<RefundRequestedPayload>(message);
        if (payload is null) return;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        var payment = await db.Payments.FirstOrDefaultAsync(
            p => p.OrderId == payload.OrderId && p.Status == PaymentStatus.Succeeded, ct);

        if (payment is null)
        {
            _logger.LogError("No succeeded payment found for order {OrderId} — cannot refund", payload.OrderId);
            return;
        }

        try
        {
            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = payment.StripePaymentIntentId,
                Amount = (long)(payload.Amount * 100) // dollars → cents
            });

            var successPayload = JsonSerializer.Serialize(new
            {
                payload.OrderId,
                payload.Amount,
                StripeRefundId = refund.Id
            });
            await publisher.PublishAsync("payments.RefundSucceeded", successPayload);
            _logger.LogInformation("Stripe refund {RefundId} of {Amount} succeeded for order {OrderId}",
                refund.Id, payload.Amount, payload.OrderId);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund of {Amount} failed for order {OrderId}", payload.Amount, payload.OrderId);
            var failPayload = JsonSerializer.Serialize(new
            {
                payload.OrderId,
                payload.Amount,
                Reason = ex.Message
            });
            await publisher.PublishAsync("payments.RefundFailed", failPayload);
        }
    }

    private record RefundRequestedPayload(Guid OrderId, decimal Amount);
}
