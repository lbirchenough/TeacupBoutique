using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class OrderCancelledConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderCancelledConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.order-cancelled";
    public string RoutingKey => "orders.OrderCancelled";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("OrderCancelled received: {Message}", message);

        var evt = JsonSerializer.Deserialize<OrderCancelledEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendOrderCancelledAsync(evt);
    }
}
