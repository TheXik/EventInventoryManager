using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Core.Entities.Rentals;

/// <summary>
/// Represents a specific inventory item included in a rental order
/// </summary>
public class RentalItem
{
    [Key]
    public int RentalItemId { get; set; }

    /// <summary>
    /// Foreign key to the parent rental order.
    /// </summary>
    [ForeignKey(nameof(Rental))]
    public int RentalId { get; set; }

    /// <summary>
    /// Navigation to the parent rental order
    /// </summary>
    public Rental Rental { get; set; }

    /// <summary>
    /// Foreign key to the inventory item being rented
    /// </summary>
    [ForeignKey(nameof(InventoryItem))]
    public int InventoryItemId { get; set; }

    /// <summary>
    /// Navigation to the inventory item being rented
    /// </summary>
    public InventoryItem InventoryItem { get; set; }

    /// <summary>
    /// Number of units rented for this line
    /// </summary>
    public int QuantityRented { get; set; }

    /// <summary>
    /// Number of units already returned for this line
    /// </summary>
    public int QuantityReturned { get; set; } = 0;

    /// <summary>
    /// Price per day for one unit recorded at the time of rental
    /// </summary>
    public decimal PricePerDayAtTimeOfRental { get; set; }
}