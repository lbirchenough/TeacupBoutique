namespace inventory.Models;

public record BookingCancelledEvent(Guid OrderId, Guid BookingId);
