namespace inventory.Models;

public class StockReservedDto
{
    public Guid OrderId { get; set; }
    public List<StockReservedItemDto> Items { get; set; } = [];
}

public class StockReservedItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DepositAmount { get; set; }
}
