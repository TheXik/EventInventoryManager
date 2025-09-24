using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Core.Entities;

/// <summary>
/// Represents a row in a truck loading plan for an event
/// </summary>
public class EventLoadingRow
{
    /// <summary>
    /// The inventory item to be loaded
    /// </summary>
    public InventoryItem Item { get; set; }

    /// <summary>
    /// Quantity of the item to load
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// The selected loading priority for the item
    /// </summary>
    public TruckLoadingPriority? SelectedPriority { get; set; }
}