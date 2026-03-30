using Messaging.Workers;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class PaymentFailedConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILoggerFactory loggerFactory) : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "notifications.payment-failed";
    public override string RoutingKey => "orders.PaymentFailed";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("PaymentFailed received: {Message}", message);

        var evt = JsonSerializer.Deserialize<PaymentFailedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendPaymentFailedAsync(evt);
    }
}
