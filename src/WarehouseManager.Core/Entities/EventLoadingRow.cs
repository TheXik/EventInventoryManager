using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Core.Entities;

public class EventLoadingRow
{

    public InventoryItem Item { get; set; }
    public int Quantity { get; set; }
    public TruckLoadingPriority? SelectedPriority { get; set; }

}