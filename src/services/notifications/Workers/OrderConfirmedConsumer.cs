using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class OrderConfirmedConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderConfirmedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.order-confirmed";
    public string RoutingKey => "orders.OrderConfirmed";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("OrderConfirmed received: {Message}", message);

        var evt = JsonSerializer.Deserialize<OrderConfirmedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendOrderConfirmedAsync(evt);
    }
}
