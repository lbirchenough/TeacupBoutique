using notifications.Models;

namespace notifications.Interfaces;

public interface INotificationService
{
    Task SendOrderConfirmedAsync(OrderConfirmedEvent evt);
    Task SendPaymentFailedAsync(PaymentFailedEvent evt);
    Task SendOrderCancelledAsync(OrderCancelledEvent evt);
    Task SendOrderCompletedAsync(OrderCompletedEvent evt);
    Task SendEmailChangedAsync(EmailChangedEvent evt);
    Task SendEmailVerificationAsync(EmailVerificationRequestedEvent evt);
    Task SendEmailChangeVerificationAsync(EmailChangeVerificationRequestedEvent evt);
}
