using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class EmailChangedConsumer(IServiceScopeFactory scopeFactory, ILogger<EmailChangedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.email-changed";
    public string RoutingKey => "auth.EmailChanged";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("EmailChanged received: {Message}", message);

        var evt = JsonSerializer.Deserialize<EmailChangedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendEmailChangedAsync(evt);
    }
}
