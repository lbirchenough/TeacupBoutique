using System;

namespace inventory.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(string topic, string message);
}
