using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class PasswordChangedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "notifications.password-changed";
    protected override string RoutingKey => "auth.PasswordChanged";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [notifications] PasswordChanged received: {message}");

        var evt = JsonSerializer.Deserialize<PasswordChangedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendPasswordChangedAsync(evt);
    }
}
