using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class PasswordResetConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "notifications.password-reset";
    protected override string RoutingKey => "auth.PasswordResetRequested";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [notifications] PasswordResetRequested received: {message}");

        var evt = JsonSerializer.Deserialize<PasswordResetRequestedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendPasswordResetAsync(evt);
    }
}
