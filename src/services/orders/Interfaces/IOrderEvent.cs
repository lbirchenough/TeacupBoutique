using System;
using orders.Entities;

namespace orders.Interfaces;

public interface IOrderEvent
{
    Task PublishOrder(Order order);
}
