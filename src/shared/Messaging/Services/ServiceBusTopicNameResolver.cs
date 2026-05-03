namespace Messaging.Services;

public static class ServiceBusTopicNameResolver
{
    public static string ToPhysicalTopicName(string logicalEventName)
    {
        if (string.IsNullOrWhiteSpace(logicalEventName))
            throw new ArgumentException("Service Bus event topic name cannot be null, empty, or whitespace.", nameof(logicalEventName));

        return logicalEventName.ToLowerInvariant();
    }
}
