namespace inventory.Models;

public record BookingCompletedEvent(
    Guid OrderId,
    Guid BookingId,
    decimal? DepositAmountKept,
    decimal RefundAmount,
    string? CompletionNotes,
    List<string> ReturnPhotoUrls);
