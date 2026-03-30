using Messaging.Workers;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class EmailVerificationConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "notifications.email-verification";
    public override string RoutingKey => "auth.EmailVerificationRequested";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("EmailVerificationRequested received: {Message}", message);

        var evt = JsonSerializer.Deserialize<EmailVerificationRequestedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendEmailVerificationAsync(evt);
    }
}
