using Messaging.Workers;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class PasswordChangedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "notifications.password-changed";
    public override string RoutingKey => "auth.PasswordChanged";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("PasswordChanged received: {Message}", message);

        var evt = JsonSerializer.Deserialize<PasswordChangedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendPasswordChangedAsync(evt);
    }
}
