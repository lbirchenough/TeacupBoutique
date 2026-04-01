using System.ComponentModel.DataAnnotations.Schema;

namespace inventory.Entities;

public class SetItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DepositValuePerUnit { get; set; }

    // Navigation properties
    public Product? Product { get; set; }
    public SpareStock? SpareStock { get; set; }
    public List<ReturnAssessment>? ReturnAssessments { get; set; }
}
