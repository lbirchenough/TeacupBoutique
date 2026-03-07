namespace inventory.Models;

public class ProductCreateDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Contents { get; set; }
    public string? Colour { get; set; }
    public decimal Price { get; set; }
    public decimal DepositAmount { get; set; }
    public int MinRentalDays { get; set; } = 1;
    public int MaxRentalDays { get; set; } = 2;
    public int BufferDays { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int Servings { get; set; }
}

