using inventory.Entities;

namespace inventory.Models;

public class ProductSetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Status Status { get; set; }
}
