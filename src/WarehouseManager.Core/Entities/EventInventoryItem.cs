using System.ComponentModel.DataAnnotations;
using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Core.Entities;

/// <summary>
/// Link entity that associates an inventory item with an event and the quantity required
/// </summary>
public class EventInventoryItem
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the related event
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Navigation property to the related event
    /// </summary>
    public Event Event { get; set; }

    /// <summary>
    /// Foreign key to the referenced inventory item
    /// </summary>
    public int InventoryItemId { get; set; }

    /// <summary>
    /// Navigation property to the referenced inventory item
    /// </summary>
    public InventoryItem InventoryItem { get; set; }

    /// <summary>
    /// Quantity of the inventory item allocated for the event
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}