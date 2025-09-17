using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Core.Entities;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ClientName { get; set; }
    public string? Location { get; set; }


    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Color { get; set; } 
    public string? RecurrenceRule { get; set; } 
    
    public EventStatus eventStatus { get; set; }
    public List<InventoryItem> ItemsNeeded { get; set; } = new();
}