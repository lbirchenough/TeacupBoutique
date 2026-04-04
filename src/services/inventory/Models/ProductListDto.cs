namespace inventory.Models;

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Colour { get; set; }
    public decimal Price { get; set; }
    public decimal DepositAmount { get; set; }
    public int Servings { get; set; }
    public string? FeaturedPhotoUrl { get; set; }
    public string? Contents { get; set; }
    public List<ProductPhotoDto> Photos { get; set; } = [];
}
