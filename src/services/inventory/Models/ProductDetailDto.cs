namespace inventory.Models;

public class ProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Contents { get; set; }
    public string? Colour { get; set; }
    public decimal Price { get; set; }
    public decimal DepositAmount { get; set; }
    public int Servings { get; set; }
    public int MinRentalDays { get; set; }
    public int MaxRentalDays { get; set; }
    public int BufferDays { get; set; }
    public bool IsActive { get; set; }
    public List<ProductPhotoDto> Photos { get; set; } = [];
}

public class ProductPhotoDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = null!;
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
}
