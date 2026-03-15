using System;
using orders.Entities;

namespace orders.Interfaces;

public interface IOrderEvent
{
    Task PublishOrder(Order order);
    Task HandleStockReserved(string orderId);
    Task HandleStockUnavailable(string orderId);
    Task HandlePaymentSucceeded(string message);
    Task HandlePaymentFailed(string message);
}
