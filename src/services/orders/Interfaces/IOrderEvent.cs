using orders.Entities;

namespace orders.Interfaces;

public interface IOrderEvent
{
    Task PublishOrder(Order order);
    Task HandleStockReserved(string orderId);
    Task HandleStockUnavailable(string orderId);
    Task HandlePaymentSucceeded(string message);
    Task HandlePaymentFailed(string message);
    Task CancelOrder(Order order, string reason);
    Task HandleBookingCancelled(string message);
    Task HandleBookingCompleted(string message);
    Task RefundOrder(Guid orderId, decimal amount);
    Task HandleRefundSucceeded(string message);
    Task HandleRefundFailed(string message);
    Task HandleReturnAssessedWithMissingItems(string message);
}
