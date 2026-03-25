using System.ComponentModel.DataAnnotations;

namespace orders.Models;

public class CreateOrderDto
{
    public Guid? UserId { get; set; }

    [Required, MaxLength(200)]
    public required string CustomerName { get; set; }

    [Required, MaxLength(200)]
    public required string CustomerEmail { get; set; }

    [Required, MaxLength(50)]
    public required string CustomerPhone { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal DepositTotal { get; set; }

    public DateOnly PickupDate { get; set; }
    public DateOnly ReturnDate { get; set; }

    public DateOnly ReservationDate { get; set; }

    [MaxLength(100)]
    public string? EventType { get; set; }

    public int? GuestCount { get; set; }
    public string? SpecialRequests { get; set; }

    [Required, MinLength(1)]
    public required List<CreateOrderItemDto> Items { get; set; }
}
