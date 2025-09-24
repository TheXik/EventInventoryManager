
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages;

/// <summary>
/// Backing class for the Dashboard page. Loads inventory and event data and exposes
/// simple computed values for the UI (Stat cards, active events, and upcoming events).
/// </summary>
public partial class Dashboard
{
    /// <summary>
    /// Currently active events (StartDate ≤ now ≤ EndDate) THey ordered by the soonest ending first.
    /// </summary>
    private List<Event> _activeEvents = new();

    /// <summary>
    /// All inventory items loaded from the repository.
    /// </summary>
    private List<InventoryItem> _allItems = new();

    /// <summary>
    /// Count of items that are currently available.
    /// </summary>
    private int _availableItemsCount;

    /// <summary>
    /// Count of items marked with <see cref="Condition.Damaged"/>.
    /// </summary>
    private int _damagedItemsCount;

    /// <summary>
    /// Count of items that have at least one unit rented out.
    /// </summary>
    private int _rentedItemsCount;

    /// <summary>
    /// Total number of inventory items.
    /// </summary>
    private int _totalItems;

    /// <summary>
    /// Upcoming events (StartDate &gt; now), ordered by start date and limited to 5.
    /// </summary>
    private List<Event> _upcomingEvents = new();

    /// <summary>
    /// Loads inventory items and events concurrently, computes KPI values, and prepares
    /// active and upcoming event collections for rendering.
    /// </summary>
    /// <returns>A task that completes when initialization is done.</returns>
    protected override async Task OnInitializedAsync()
    {
        var itemsTask = InventoryRepo.GetAllAsync();
        var eventsTask = EventRepo.GetAllAsync();
        await Task.WhenAll(itemsTask, eventsTask);

        _allItems = (await itemsTask).ToList();
        var allEvents = (await eventsTask).ToList();

        _totalItems = _allItems.Count;
        _rentedItemsCount = _allItems.Count(i => i.AvailableQuantity < i.TotalQuantity);
        _damagedItemsCount = _allItems.Count(i => i.Condition == Condition.Damaged);
        _availableItemsCount = _allItems.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available);

        var now = DateTime.Now;
        _activeEvents = allEvents
            .Where(e => e.StartDate <= now && e.EndDate >= now)
            .OrderBy(e => e.EndDate)
            .ToList();

        _upcomingEvents = allEvents
            .Where(e => e.StartDate > now)
            .OrderBy(e => e.StartDate)
            .Take(5)
            .ToList();
    }

    /// <summary>
    /// Returns the Bootstrap-like badge CSS class string for the given due date.
    /// </summary>
    /// <param name="dueDate">The event's end date (used as the rental due date).</param>
    /// <returns>
    /// <c>"bg-danger"</c> when the due date is in the past (overdue), otherwise <c>"bg-success"</c>.
    /// </returns>
    private string GetRentalStatusBadge(DateTime dueDate)
    {
        return dueDate < DateTime.Now.Date ? "bg-danger" : "bg-success";
    }

    /// <summary>
    /// Returns the human-readable status text for the given due date.
    /// </summary>
    /// <param name="dueDate">The event's end date (used as the rental due date).</param>
    /// <returns><c>"Overdue"</c> if the date is past today; otherwise <c>"Active"</c>.</returns>
    private string GetRentalStatusText(DateTime dueDate)
    {
        return dueDate < DateTime.Now.Date ? "Overdue" : "Active";
    }
}