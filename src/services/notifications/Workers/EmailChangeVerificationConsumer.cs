using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class EmailChangeVerificationConsumer(IServiceScopeFactory scopeFactory, ILogger<EmailChangeVerificationConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.email-change-verification";
    public string RoutingKey => "auth.EmailChangeVerificationRequested";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("EmailChangeVerificationRequested received: {Message}", message);

        var evt = JsonSerializer.Deserialize<EmailChangeVerificationRequestedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendEmailChangeVerificationAsync(evt);
    }
}
