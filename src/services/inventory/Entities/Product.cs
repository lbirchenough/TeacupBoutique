using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace inventory.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Contents { get; set; }
    public string? Colour { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DepositAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MinRentalDays { get; set; } = 1;
    public int MaxRentalDays { get; set; } = 2;
    public int BufferDays { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int Servings { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public List<Photo>? Photos { get; set; }
    public List<Tag>? Tags { get; set; }
    public List<ProductSet>? ProductSets { get; set; }
    public List<SetItem>? SetItems { get; set; }
    public List<BookingItem>? BookingItems { get; set; }
}
