using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
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
    // Constants
    private const int MaxUpcomingEventsDisplay = 5;
    
    // State
    private bool _isLoading = true;
    private string? _errorMessage;
    
    // Data collections
    private List<Event> _activeEvents = new();
    private List<InventoryItem> _allItems = new();
    private List<Event> _upcomingEvents = new();
    
    // Computed statistics
    private DashboardStatistics _statistics = new();
    
    // Dependencies
    [Inject] private ILogger<Dashboard> Logger { get; set; } = default!;

    /// <summary>
    /// Loads inventory items and events concurrently, computes KPI values, and prepares
    /// active and upcoming event collections for rendering.
    /// </summary>
    /// <returns>A task that completes when initialization is done.</returns>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            await LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load dashboard data");
            _errorMessage = "Failed to load dashboard data. Please refresh the page.";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
    
    /// <summary>
    /// Loads and processes dashboard data concurrently.
    /// </summary>
    private async Task LoadDashboardDataAsync()
    {
        var itemsTask = InventoryRepo.GetAllAsync();
        var eventsTask = EventRepo.GetAllAsync();
        await Task.WhenAll(itemsTask, eventsTask);

        _allItems = (await itemsTask).ToList();
        var allEvents = (await eventsTask).ToList();

        ComputeStatistics();
        ProcessEvents(allEvents);
    }
    
    /// <summary>
    /// Computes dashboard statistics from inventory data.
    /// </summary>
    private void ComputeStatistics()
    {
        _statistics = new DashboardStatistics
        {
            TotalItems = _allItems.Count,
            AvailableItemsCount = _allItems.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
            RentedItemsCount = _allItems.Count(i => i.AvailableQuantity < i.TotalQuantity),
            DamagedItemsCount = _allItems.Count(i => i.Condition == Condition.Damaged)
        };
    }
    
    /// <summary>
    /// Processes events to separate active and upcoming events.
    /// </summary>
    private void ProcessEvents(List<Event> allEvents)
    {
        var now = DateTime.Now;
        
        _activeEvents = allEvents
            .Where(e => e.StartDate <= now && e.EndDate >= now)
            .OrderBy(e => e.EndDate)
            .ToList();

        _upcomingEvents = allEvents
            .Where(e => e.StartDate > now)
            .OrderBy(e => e.StartDate)
            .Take(MaxUpcomingEventsDisplay)
            .ToList();
    }

    /// <summary>
    /// Returns the Bootstrap-like badge CSS class string for the given due date.
    /// </summary>
    /// <param name="dueDate">The event's end date (used as the rental due date).</param>
    /// <returns>
    /// <c>"bg-danger"</c> when the due date is in the past (overdue), otherwise <c>"bg-success"</c>.
    /// </returns>
    private static string GetRentalStatusBadge(DateTime dueDate)
    {
        return dueDate < DateTime.Now.Date ? "bg-danger" : "bg-success";
    }

    /// <summary>
    /// Returns the human-readable status text for the given due date.
    /// </summary>
    /// <param name="dueDate">The event's end date (used as the rental due date).</param>
    /// <returns><c>"Overdue"</c> if the date is past today; otherwise <c>"Active"</c>.</returns>
    private static string GetRentalStatusText(DateTime dueDate)
    {
        return dueDate < DateTime.Now.Date ? "Overdue" : "Active";
    }
    
    /// <summary>
    /// Refreshes the dashboard data.
    /// </summary>
    public async Task RefreshDataAsync()
    {
        await OnInitializedAsync();
    }
}

/// <summary>
/// View model containing dashboard statistics.
/// </summary>
public class DashboardStatistics
{
    public int TotalItems { get; set; }
    public int AvailableItemsCount { get; set; }
    public int RentedItemsCount { get; set; }
    public int DamagedItemsCount { get; set; }
}