using Microsoft.AspNetCore.Components;
using WarehouseManager.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Infrastructure.Repositories;

namespace WarehouseManager.Web.Components.Pages.EventsDir
{
    public partial class EventInventory
    {
        [Parameter] public int EventId { get; set; }

        [Inject] private IEventRepository _EventRepository { get; set; }

        [Inject] private IInventoryItemRepository _InventoryItemRepository { get; set; }

        [Inject] private IEventInventoryItemRepository _EventInventoryItemRepository { get; set; } // <-- Add this

        [Inject] private NavigationManager _NavigationManager { get; set; }


        private Event _event;
        private List<InventoryItem> allItems;
        private List<InventoryItem> availableItems;
        private List<EventInventoryItem> selectedItems;
        private string searchTerm = "";
        private string? _errorMessage;


        protected override async Task OnInitializedAsync()
        {
            _event = await EventRepository.GetByIdAsync(EventId);
            allItems = (await InventoryItemRepository.GetAllAsync()).ToList();
            selectedItems = _event.EventInventoryItems?.ToList() ?? new List<EventInventoryItem>();
            FilterItems();
        }

        private void FilterItems()
        {
            var selectedIds = selectedItems.Select(i => i.InventoryItemId);
            availableItems = allItems
                .Where(i => !selectedIds.Contains(i.Id) &&
                            i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void AddItemToEvent(InventoryItem item)
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
            }
        }

        private void RemoveItemFromEvent(EventInventoryItem eventItem)
        {
            selectedItems.Remove(eventItem);
            FilterItems();
        }

        private int GetMaxQuantityForItem(InventoryItem item)
        {
            var selected = selectedItems.FirstOrDefault(si => si.InventoryItemId == item.Id);
            if (selected != null)
            {
                return item.AvailableQuantity + selected.Quantity;
            }

            return item.AvailableQuantity;
        }

        private void CorrectQuantity(EventInventoryItem item, ChangeEventArgs e, int max)
        {
            if (int.TryParse(e.Value?.ToString(), out int value))
            {
                if (value < 1)
                {
                    item.Quantity = 1; // Immediately reset to 1 if the user enters 0 or less
                }
                else if (value > max)
                {
                    item.Quantity = max; // Immediately reset to the max if the user enters a higher number
                }
            }
        }

        private async Task SaveChanges()
        {
            _errorMessage = null; // Clear any previous errors

            // Get the original list of items assigned to this event
            var originalItems = (await _EventInventoryItemRepository.GetByEventIdAsync(EventId)).ToList();

            // --- VALIDATION STEP ---
            // First, check if the requested changes are possible before making any database calls.
            foreach (var selectedItem in selectedItems)
            {
                var itemInWarehouse = await InventoryItemRepository.GetByIdAsync(selectedItem.InventoryItemId);
                var originalItem =
                    originalItems.FirstOrDefault(oi => oi.InventoryItemId == selectedItem.InventoryItemId);

                // Calculate how many *more* items are being requested than before.
                int quantityChange = selectedItem.Quantity - (originalItem?.Quantity ?? 0);

                if (quantityChange > itemInWarehouse.AvailableQuantity)
                {
                    _errorMessage = $"Cannot save changes. Not enough '{itemInWarehouse.Name}' in stock. " +
                                    $"You are trying to assign {quantityChange} more, but only {itemInWarehouse.AvailableQuantity} are available.";
                    StateHasChanged();
                    return; // Stop the entire save process
                }
            }

            // --- If validation passes, proceed with saving ---

            // Return items that were fully removed from the event
            foreach (var originalItem in originalItems.Where(oi =>
                         !selectedItems.Any(si => si.InventoryItemId == oi.InventoryItemId)))
            {
                var item = await InventoryItemRepository.GetByIdAsync(originalItem.InventoryItemId);
                item.AvailableQuantity += originalItem.Quantity;
                item.UpdateAvailabilityStatus();
                await InventoryItemRepository.UpdateAsync(item);
            }

            // Update quantities for items that were added or changed
            foreach (var selectedItem in selectedItems)
            {
                var item = await InventoryItemRepository.GetByIdAsync(selectedItem.InventoryItemId);
                var originalItem =
                    originalItems.FirstOrDefault(oi => oi.InventoryItemId == selectedItem.InventoryItemId);
                int quantityDifference = selectedItem.Quantity - (originalItem?.Quantity ?? 0);

                if (quantityDifference != 0)
                {
                    item.AvailableQuantity -= quantityDifference;
                    item.UpdateAvailabilityStatus();
                    await InventoryItemRepository.UpdateAsync(item);
                }
            }

            // Finally, save the updated list of items for the event
            _event.EventInventoryItems = selectedItems;
            await EventRepository.UpdateAsync(_event);

            NavigationManager.NavigateTo("/events");
        }
    }
}