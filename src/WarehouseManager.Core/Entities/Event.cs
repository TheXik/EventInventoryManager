using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Core.Entities;

public class Event
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string? ClientName { get; set; }
    public string? Location { get; set; }
    
    public List<InventoryItem> ItemsNeeded { get; set; } = new();
}