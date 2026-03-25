using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class OrderCompletedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "notifications.order-completed";
    protected override string RoutingKey => "orders.OrderCompleted";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [notifications] OrderCompleted received: {message}");

        var evt = JsonSerializer.Deserialize<OrderCompletedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendOrderCompletedAsync(evt);
    }
}
