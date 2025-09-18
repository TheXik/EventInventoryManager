using Microsoft.AspNetCore.Components;
using WarehouseManager.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Web.Components.Pages.EventsDir
{
    public partial class EventInventory
    {
        [Parameter]
        public int EventId { get; set; }

        [Inject]
        private IEventRepository _EventRepository { get; set; }
        
        [Inject]
        private IInventoryItemRepository _InventoryItemRepository { get; set; }

        [Inject]
        private IEventInventoryItemRepository _EventInventoryItemRepository { get; set; } // <-- Add this

        [Inject]
        private NavigationManager _NavigationManager { get; set; }


        private Event _event;
        private List<InventoryItem> allItems;
        private List<InventoryItem> availableItems;
        private List<EventInventoryItem> selectedItems;
        private string searchTerm = "";

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
                .Where(i => !selectedIds.Contains(i.Id) && i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
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
                // The max quantity is the amount currently available in the warehouse
                // PLUS the amount already assigned to this event.
                return item.AvailableQuantity + selected.Quantity;
            }
            return item.AvailableQuantity;
        }

        private async Task SaveChanges()
        {
            // Get the original items for this event before any changes
            var originalItems = (await _EventInventoryItemRepository.GetByEventIdAsync(EventId)).ToList();

            // Return items that were removed
            foreach (var originalItem in originalItems)
            {
                if (!selectedItems.Any(si => si.InventoryItemId == originalItem.InventoryItemId))
                {
                    var item = await InventoryItemRepository.GetByIdAsync(originalItem.InventoryItemId);
                    item.AvailableQuantity += originalItem.Quantity;
                    item.UpdateAvailabilityStatus();
                    await InventoryItemRepository.UpdateAsync(item);
                }
            }

            // Add or update items
            foreach (var selectedItem in selectedItems)
            {
                var originalItem = originalItems.FirstOrDefault(oi => oi.InventoryItemId == selectedItem.InventoryItemId);
                var item = await InventoryItemRepository.GetByIdAsync(selectedItem.InventoryItemId);

                if (originalItem != null)
                {
                    // The item was already in the event, so we just adjust the quantity
                    int quantityDifference = selectedItem.Quantity - originalItem.Quantity;
                    item.AvailableQuantity -= quantityDifference;
                }
                else
                {
                    // This is a new item for the event
                    item.AvailableQuantity -= selectedItem.Quantity;
                }

                item.UpdateAvailabilityStatus();
                await InventoryItemRepository.UpdateAsync(item);
            }

            // Now, save the changes to the event's inventory list
            _event.EventInventoryItems = selectedItems;
            await EventRepository.UpdateAsync(_event);

            NavigationManager.NavigateTo("/events");
        }
    }
}