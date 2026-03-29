namespace orders.Models;

public record ReadyForPaymentDto(Guid OrderId, decimal Amount);
