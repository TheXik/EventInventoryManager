using System.ComponentModel.DataAnnotations;
using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Core.Entities;

public class EventInventoryItem
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; }
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}