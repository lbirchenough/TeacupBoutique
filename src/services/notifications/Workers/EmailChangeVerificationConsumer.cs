using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class EmailChangeVerificationConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "notifications.email-change-verification";
    protected override string RoutingKey => "auth.EmailChangeVerificationRequested";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [notifications] EmailChangeVerificationRequested received: {message}");

        var evt = JsonSerializer.Deserialize<EmailChangeVerificationRequestedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendEmailChangeVerificationAsync(evt);
    }
}
