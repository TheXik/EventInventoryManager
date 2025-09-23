using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Core.Entities;

public class RentalRow
{
    public InventoryItem Item { get; set; }
    public int Quantity { get; set; }
    public decimal PricePerDay { get; set; }
}