using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

/// <summary>
/// Component for managing loading priorities and generating loading plans for events
/// Provides truck loading optimization and printable loading lists
/// </summary>
public partial class EventLoading
{
    // Data and UI State
    private Event _event = default!;
    private List<EventLoadingRow> _rows = new();
    private string? _errorMessage;
    private bool _isLoading = true;

    // Parameters
    [Parameter] public int EventId { get; set; }

    // Dependency Injection
    [Inject] private IEventRepository EventRepository { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryItemRepository { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<EventLoading> Logger { get; set; } = default!;

    /// <summary>
    /// Computed property that returns rows sorted by priority and then by item name
    /// </summary>
    private IEnumerable<EventLoadingRow> SortedRows => _rows
        .OrderByDescending(r => r.SelectedPriority.HasValue ? (int)r.SelectedPriority.Value : -1)
        .ThenBy(r => r.Item.Name);

    /// <summary>
    /// Initializes the component by loading event data and creating loading rows
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            var fetched = await EventRepository.GetByIdAsync(EventId);
            if (fetched == null)
            {
                _errorMessage = "Event not found.";
                NavigationManager.NavigateTo("/events");
                return;
            }

            _event = fetched;
            var allItems = (await InventoryItemRepository.GetAllAsync()).ToList();

            var eventItems = _event.EventInventoryItems?.ToList() ?? new List<EventInventoryItem>();
            var inventoryById = allItems.ToDictionary(i => i.Id, i => i);

            _rows = new List<EventLoadingRow>();
            foreach (var ei in eventItems)
            {
                if (inventoryById.TryGetValue(ei.InventoryItemId, out var inventoryItem))
                {
                    _rows.Add(new EventLoadingRow
                    {
                        Item = inventoryItem,
                        Quantity = ei.Quantity,
                        SelectedPriority = inventoryItem.TruckLoadingPriority
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load event loading data: {ex.Message}";
            Logger.LogError(ex, "Error loading event loading data for event {EventId}", EventId);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Saves the updated loading priorities to the database
    /// </summary>
    private async Task SavePriorities()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            foreach (var row in _rows)
            {
                if (row.Item.TruckLoadingPriority != row.SelectedPriority)
                {
                    row.Item.TruckLoadingPriority = row.SelectedPriority;
                    await InventoryItemRepository.UpdateAsync(row.Item);
                }
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save priorities: {ex.Message}";
            Logger.LogError(ex, "Error saving loading priorities for event {EventId}", EventId);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Navigates back to the event inventory management page
    /// </summary>
    private void BackToInventory()
    {
        NavigationManager.NavigateTo($"/event-inventory/{EventId}");
    }

    /// <summary>
    /// Saves priorities and triggers browser print dialog for the loading list
    /// </summary>
    private async Task DownloadList()
    {
        try
        {
            // Ensure latest priorities are saved
            await SavePriorities();

            await JS.InvokeVoidAsync("print");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to print loading list: {ex.Message}";
            Logger.LogError(ex, "Error printing loading list for event {EventId}", EventId);
        }
    }
}