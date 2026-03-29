using System.ComponentModel.DataAnnotations;

namespace orders.Models;

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public DateOnly RentalDate { get; set; }

    [MaxLength(200)]
    public required string ProductName { get; set; }

    [MaxLength(100)]
    public string? ProductThemeColor { get; set; }

    [MaxLength(500)]
    public string? ProductImageUrl { get; set; }


}
