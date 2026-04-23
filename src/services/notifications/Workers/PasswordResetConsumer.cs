using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class PasswordResetConsumer(IServiceScopeFactory scopeFactory, ILogger<PasswordResetConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.password-reset";
    public string RoutingKey => "auth.PasswordResetRequested";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("PasswordResetRequested received: {Message}", message);

        var evt = JsonSerializer.Deserialize<PasswordResetRequestedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendPasswordResetAsync(evt);
    }
}
