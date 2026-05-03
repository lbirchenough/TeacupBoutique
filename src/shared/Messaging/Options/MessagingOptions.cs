namespace Messaging.Options;

public class MessagingOptions
{
    public const string RabbitMq = "RabbitMq";
    public const string ServiceBus = "ServiceBus";

    public string Provider { get; set; } = RabbitMq;
}
