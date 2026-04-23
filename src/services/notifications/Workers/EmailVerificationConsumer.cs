using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class EmailVerificationConsumer(IServiceScopeFactory scopeFactory, ILogger<EmailVerificationConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.email-verification";
    public string RoutingKey => "auth.EmailVerificationRequested";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("EmailVerificationRequested received: {Message}", message);

        var evt = JsonSerializer.Deserialize<EmailVerificationRequestedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendEmailVerificationAsync(evt);
    }
}
