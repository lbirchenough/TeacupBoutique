namespace Messaging.Interfaces;

public interface IMessageConsumer
{
    string QueueName { get; }
    string RoutingKey { get; }
    Task HandleMessageAsync(string message, CancellationToken ct);
}
