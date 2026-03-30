using Messaging.Workers;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class OrderCompletedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "notifications.order-completed";
    public override string RoutingKey => "orders.OrderCompleted";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("OrderCompleted received: {Message}", message);

        var evt = JsonSerializer.Deserialize<OrderCompletedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendOrderCompletedAsync(evt);
    }
}
