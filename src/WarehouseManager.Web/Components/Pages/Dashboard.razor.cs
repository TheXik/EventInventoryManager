using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages;

public partial class Dashboard
{
    private List<InventoryItem> _allItems = new();
    private int _totalItems;
    private int _rentedItemsCount;
    private int _damagedItemsCount;
    private int _availableItemsCount;

    private List<Event> _activeEvents = new();
    private List<Event> _upcomingEvents = new();

    protected override async Task OnInitializedAsync()
    {
        var itemsTask = InventoryRepo.GetAllAsync();
        var eventsTask = EventRepo.GetAllAsync();
        await Task.WhenAll(itemsTask, eventsTask);

        _allItems = (await itemsTask).ToList();
        var allEvents = (await eventsTask).ToList();

        _totalItems = _allItems.Count;
        _rentedItemsCount = _allItems.Count(i => i.RentalStatus == RentalStatus.Rented);
        _damagedItemsCount = _allItems.Count(i => i.Condition == Condition.Damaged);
        _availableItemsCount = _allItems.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available);

        var now = DateTime.Now;
        _activeEvents = allEvents.Where(e => e.StartDate <= now && e.EndDate >= now).OrderBy(e => e.EndDate).ToList();
        _upcomingEvents = allEvents.Where(e => e.StartDate > now).OrderBy(e => e.StartDate).Take(5).ToList();
    }

    private string GetRentalStatusBadge(DateTime dueDate)
    {
        return dueDate < DateTime.Now.Date ? "bg-danger" : "bg-success";
    }

    private string GetRentalStatusText(DateTime dueDate)
    {
        return dueDate < DateTime.Now.Date ? "Overdue" : "Active";
    }

}