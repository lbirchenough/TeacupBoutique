namespace orders.Models;

public record BookingCancelledEvent(Guid OrderId, Guid BookingId);
