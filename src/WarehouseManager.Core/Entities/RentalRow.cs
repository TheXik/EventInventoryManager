using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Core.Entities;

/// <summary>
/// Represents a single line item within a rental
/// </summary>
public class RentalRow
{
    /// <summary>
    /// Inventory item being rented
    /// </summary>
    public InventoryItem Item { get; set; }

    /// <summary>
    /// Number of units to rent
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price charged per day for a single unit
    /// </summary>
    public decimal PricePerDay { get; set; }
}