using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

/// <summary>
/// Component for managing inventory items assigned to a specific event
/// Provides drag-and-drop interface for adding/removing items and quantity management
/// </summary>
public partial class EventInventory
{
    // Data and UI State
    private string? _errorMessage;
    private Event _event = default!;
    private List<InventoryItem> allItems = new();
    private List<InventoryItem> availableItems = new();
    private string searchTerm = "";
    private List<EventInventoryItem> selectedItems = new();
    private bool _isLoading = true;

    // Parameters
    [Parameter] public int EventId { get; set; }

    // Dependency Injection
    [Inject] private IEventRepository _EventRepository { get; set; } = default!;
    [Inject] private IInventoryItemRepository _InventoryItemRepository { get; set; } = default!;
    [Inject] private IEventInventoryItemRepository _EventInventoryItemRepository { get; set; } = default!;
    [Inject] private NavigationManager _NavigationManager { get; set; } = default!;
    [Inject] private ILogger<EventInventory> Logger { get; set; } = default!;


    /// <summary>
    /// Initializes the component by loading event and inventory data
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            var fetched = await _EventRepository.GetByIdAsync(EventId);
            if (fetched == null)
            {
                _errorMessage = "Event not found.";
                _NavigationManager.NavigateTo("/events");
                return;
            }

            _event = fetched;
            allItems = (await _InventoryItemRepository.GetAllAsync()).ToList();
            selectedItems = _event.EventInventoryItems?.ToList() ?? new List<EventInventoryItem>();
            FilterItems();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load event inventory: {ex.Message}";
            Logger.LogError(ex, "Error loading event inventory for event {EventId}", EventId);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Filters available items based on search term and excludes already selected items
    /// </summary>
    private void FilterItems()
    {
        var selectedIds = selectedItems.Select(i => i.InventoryItemId).ToHashSet();
        availableItems = allItems
            .Where(i => !selectedIds.Contains(i.Id) &&
                       (string.IsNullOrWhiteSpace(searchTerm) ||
                        i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(i => i.Name)
            .ToList();
    }

    /// <summary>
    /// Adds an inventory item to the event with default quantity of 1
    /// </summary>
    /// <param name="item">The inventory item to add</param>
    private void AddItemToEvent(InventoryItem item)
    {
        try
        {
            if (!selectedItems.Any(ei => ei.InventoryItemId == item.Id))
            {
                selectedItems.Add(new EventInventoryItem
                {
                    EventId = EventId,
                    Event = _event,
                    InventoryItemId = item.Id,
                    InventoryItem = item,
                    Quantity = 1
                });
                FilterItems();
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to add item: {ex.Message}";
            Logger.LogError(ex, "Error adding item {ItemId} to event {EventId}", item.Id, EventId);
        }
    }

    /// <summary>
    /// Removes an inventory item from the event
    /// </summary>
    /// <param name="eventItem">The event inventory item to remove</param>
    private void RemoveItemFromEvent(EventInventoryItem eventItem)
    {
        try
        {
            selectedItems.Remove(eventItem);
            FilterItems();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to remove item: {ex.Message}";
            Logger.LogError(ex, "Error removing item {ItemId} from event {EventId}", eventItem.InventoryItemId, EventId);
        }
    }

    /// <summary>
    /// Calculates the maximum quantity available for an item
    /// </summary>
    /// <param name="item">The inventory item</param>
    /// <returns>Maximum available quantity</returns>
    private int GetMaxQuantityForItem(InventoryItem item)
    {
        var selected = selectedItems.FirstOrDefault(si => si.InventoryItemId == item.Id);
        if (selected != null) 
            return item.AvailableQuantity + selected.Quantity;

        return item.AvailableQuantity;
    }

    /// <summary>
    /// Validates and corrects quantity input to ensure it stays within valid bounds
    /// </summary>
    /// <param name="item">The event inventory item</param>
    /// <param name="e">The change event arguments</param>
    /// <param name="max">The maximum allowed quantity</param>
    private void CorrectQuantity(EventInventoryItem item, ChangeEventArgs e, int max)
    {
        try
        {
            if (int.TryParse(e.Value?.ToString(), out var value))
            {
                if (value < 1)
                    item.Quantity = 1;
                else if (value > max)
                    item.Quantity = max;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error correcting quantity for item {ItemId}", item.InventoryItemId);
        }
    }

    /// <summary>
    /// Saves changes to the event inventory with comprehensive validation.
    /// Updates inventory availability and event item assignments.
    /// </summary>
    private async Task SaveChanges()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            // Get the original list of items assigned to this event
            var originalItems = (await _EventInventoryItemRepository.GetByEventIdAsync(EventId)).ToList();

            // Check if requested changes are possible
            foreach (var selectedItem in selectedItems)
            {
                var itemInWarehouse = await _InventoryItemRepository.GetByIdAsync(selectedItem.InventoryItemId);
                var originalItem = originalItems.FirstOrDefault(oi => oi.InventoryItemId == selectedItem.InventoryItemId);

                // Calculate quantity change
                var quantityChange = selectedItem.Quantity - (originalItem?.Quantity ?? 0);

                if (itemInWarehouse == null)
                {
                    _errorMessage = "Cannot save changes. The selected item was not found in inventory.";
                    return;
                }

                if (quantityChange > itemInWarehouse.AvailableQuantity)
                {
                    _errorMessage = $"Cannot save changes. Not enough '{itemInWarehouse.Name}' in stock. " +
                                    $"You are trying to assign {quantityChange} more, but only {itemInWarehouse.AvailableQuantity} are available.";
                    return;
                }
            }

            // Return items that were fully removed from the event
            foreach (var originalItem in originalItems.Where(oi =>
                         !selectedItems.Any(si => si.InventoryItemId == oi.InventoryItemId)))
            {
                var item = await _InventoryItemRepository.GetByIdAsync(originalItem.InventoryItemId);
                if (item != null)
                {
                    item.AvailableQuantity += originalItem.Quantity;
                    item.UpdateAvailabilityStatus();
                    await _InventoryItemRepository.UpdateAsync(item);
                }
            }

            // Update quantities for items that were added or changed
            foreach (var selectedItem in selectedItems)
            {
                var item = await _InventoryItemRepository.GetByIdAsync(selectedItem.InventoryItemId);
                var originalItem = originalItems.FirstOrDefault(oi => oi.InventoryItemId == selectedItem.InventoryItemId);
                var quantityDifference = selectedItem.Quantity - (originalItem?.Quantity ?? 0);

                if (item != null && quantityDifference != 0)
                {
                    item.AvailableQuantity -= quantityDifference;
                    item.UpdateAvailabilityStatus();
                    await _InventoryItemRepository.UpdateAsync(item);
                }
            }

            // Save the updated list of items for the event
            _event.EventInventoryItems = selectedItems;
            await _EventRepository.UpdateAsync(_event);

            _NavigationManager.NavigateTo("/events");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save changes: {ex.Message}";
            Logger.LogError(ex, "Error saving event inventory changes for event {EventId}", EventId);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}