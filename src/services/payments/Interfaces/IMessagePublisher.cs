namespace payments.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(string topic, string message);
}
