using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class PasswordChangedConsumer(IServiceScopeFactory scopeFactory, ILogger<PasswordChangedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.password-changed";
    public string RoutingKey => "auth.PasswordChanged";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("PasswordChanged received: {Message}", message);

        var evt = JsonSerializer.Deserialize<PasswordChangedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendPasswordChangedAsync(evt);
    }
}
