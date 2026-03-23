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
