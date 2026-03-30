using Messaging.Workers;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class OrderConfirmedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "notifications.order-confirmed";
    public override string RoutingKey => "orders.OrderConfirmed";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("OrderConfirmed received: {Message}", message);

        var evt = JsonSerializer.Deserialize<OrderConfirmedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendOrderConfirmedAsync(evt);
    }
}
