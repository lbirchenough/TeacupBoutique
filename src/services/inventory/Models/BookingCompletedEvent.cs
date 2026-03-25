namespace inventory.Models;

public record BookingCompletedEvent(
    Guid OrderId,
    Guid BookingId,
    decimal? DepositAmountKept,
    string? CompletionNotes,
    List<string> ReturnPhotoUrls);
