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
    // public string? Size { get; set; }
    // public string? Material { get; set; }
    // public string? Style { get; set; }
    // public string? Pattern { get; set; }
    // public string? Brand { get; set; }
    // public string? Model { get; set; }
    // public string? Series { get; set; }

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
    public List<InventoryItem>? InventoryItems { get; set; }
    public List<BookingItem>? BookingItems { get; set; }

    
    
}
