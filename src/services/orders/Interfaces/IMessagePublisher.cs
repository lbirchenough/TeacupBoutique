using System;

namespace orders.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(string topic, string message);
}
