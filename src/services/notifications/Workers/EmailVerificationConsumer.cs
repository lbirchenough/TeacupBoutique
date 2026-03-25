using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class EmailVerificationConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "notifications.email-verification";
    protected override string RoutingKey => "auth.EmailVerificationRequested";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [notifications] EmailVerificationRequested received: {message}");

        var evt = JsonSerializer.Deserialize<EmailVerificationRequestedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendEmailVerificationAsync(evt);
    }
}
