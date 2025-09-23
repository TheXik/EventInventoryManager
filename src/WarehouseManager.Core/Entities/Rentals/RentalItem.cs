using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Core.Entities.Rentals;

public class RentalItem
{
    [Key]
    public int RentalItemId { get; set; }

    [ForeignKey(nameof(Rental))]
    public int RentalId { get; set; }
    public Rental Rental { get; set; }

    [ForeignKey(nameof(InventoryItem))]
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; }

    public int QuantityRented { get; set; }
    public int QuantityReturned { get; set; } = 0;

    public decimal PricePerDayAtTimeOfRental { get; set; }
}