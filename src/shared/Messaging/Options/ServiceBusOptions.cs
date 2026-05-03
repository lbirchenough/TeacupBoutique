namespace Messaging.Options;

public class ServiceBusOptions
{
    public string? ConnectionString { get; set; }
    public string? FullyQualifiedNamespace { get; set; }

    // Maps a logical event name (e.g. "Orders.OrderPlaced") to the queue(s) it is sent to.
    // Multiple queues = publisher-side fan-out (replaces topic subscriptions on Basic SKU).
    public Dictionary<string, string[]> EventTargets { get; set; } = new();
}
