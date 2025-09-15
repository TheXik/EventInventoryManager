using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.ViewModels;

public class InventoryFilterViewModel
{
    public HashSet<int> SelectedCategoryIds { get; set; } = new();
    public HashSet<AvailabilityStatus> SelectedAvailabilityStatuses { get; set; } = new();
    public HashSet<Condition> SelectedConditions { get; set; } = new();
}