using System;


using inventory.Entities;
namespace inventory.Models;

public class InventoryItemsDto
{
    public Guid Id { get; set; }
    public Condition Condition { get; set; }
    public Status Status { get; set; }
    public string? ConditionNotes { get; set; }
    public string? MaintenanceHistory { get; set; }
}
