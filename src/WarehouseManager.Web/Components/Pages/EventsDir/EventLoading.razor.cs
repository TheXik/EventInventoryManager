using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.EventsDir
{
    public partial class EventLoading
    {
        [Parameter] public int EventId { get; set; }

        [Inject] private IEventRepository EventRepository { get; set; }
        [Inject] private IInventoryItemRepository InventoryItemRepository { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJSRuntime JS { get; set; }

        private Event _event;

        private List<EventLoadingRow> _rows = new();

        private IEnumerable<EventLoadingRow> SortedRows => _rows
            .OrderByDescending(r => r.SelectedPriority.HasValue ? (int)r.SelectedPriority.Value : -1)
            .ThenBy(r => r.Item.Name);

        protected override async Task OnInitializedAsync()
        {
            _event = await EventRepository.GetByIdAsync(EventId);
            var allItems = (await InventoryItemRepository.GetAllAsync()).ToList();

            var eventItems = _event?.EventInventoryItems?.ToList() ?? new List<EventInventoryItem>();
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

        private async Task SavePriorities()
        {
            foreach (var row in _rows)
            {
                if (row.Item.TruckLoadingPriority != row.SelectedPriority)
                {
                    row.Item.TruckLoadingPriority = row.SelectedPriority;
                    await InventoryItemRepository.UpdateAsync(row.Item);
                }
            }
        }

        private void BackToInventory()
        {
            NavigationManager.NavigateTo($"/event-inventory/{EventId}");
        }

        private async Task DownloadList()
        {
            // Ensure latest priorities are saved
            await SavePriorities();

            try
            {
                await JS.InvokeVoidAsync("print");
            }
            catch
            {
                // TODO show some errors
            }
        }

        
    }
}
