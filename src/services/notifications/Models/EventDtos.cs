namespace notifications.Models;

public record OrderConfirmedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid AccessToken,
    string CustomerName,
    string CustomerEmail,
    decimal Total,
    DateOnly ReservationDate,
    List<OrderConfirmedItemEvent> Items);

public record OrderConfirmedItemEvent(string Name, int Quantity, decimal UnitPrice);

public record PaymentFailedEvent(
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DateOnly ReservationDate,
    int Attempt);

public record OrderCancelledEvent(
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DateOnly ReservationDate);

public record EmailChangedEvent(string UserId, string FullName, string OldEmail, string NewEmail);

public record EmailVerificationRequestedEvent(string UserId, string Email, string VerificationLink);
public record EmailChangeVerificationRequestedEvent(string UserId, string NewEmail, string VerificationLink);
public record PasswordResetRequestedEvent(string Email, string ResetLink);
public record PasswordChangedEvent(string Email);

public record OrderCompletedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid AccessToken,
    string CustomerName,
    string CustomerEmail,
    DateOnly ReservationDate,
    decimal? DepositAmountKept,
    string? CompletionNotes);
