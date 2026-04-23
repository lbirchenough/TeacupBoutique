using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class OrderCompletedConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderCompletedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.order-completed";
    public string RoutingKey => "orders.OrderCompleted";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("OrderCompleted received: {Message}", message);

        var evt = JsonSerializer.Deserialize<OrderCompletedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendOrderCompletedAsync(evt);
    }
}
