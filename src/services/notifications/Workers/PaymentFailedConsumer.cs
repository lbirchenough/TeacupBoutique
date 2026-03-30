using Messaging.Workers;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class PaymentFailedConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    public override string QueueName => "notifications.payment-failed";
    public override string RoutingKey => "orders.PaymentFailed";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [notifications] PaymentFailed received: {message}");

        var evt = JsonSerializer.Deserialize<PaymentFailedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendPaymentFailedAsync(evt);
    }
}
