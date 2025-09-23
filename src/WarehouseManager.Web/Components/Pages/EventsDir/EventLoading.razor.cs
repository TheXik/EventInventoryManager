using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

public partial class EventLoading
{
    private Event _event = default!;

    private List<EventLoadingRow> _rows = new();
    [Parameter] public int EventId { get; set; }

    [Inject] private IEventRepository EventRepository { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryItemRepository { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IEnumerable<EventLoadingRow> SortedRows => _rows
        .OrderByDescending(r => r.SelectedPriority.HasValue ? (int)r.SelectedPriority.Value : -1)
        .ThenBy(r => r.Item.Name);

    protected override async Task OnInitializedAsync()
    {
        var fetched = await EventRepository.GetByIdAsync(EventId);
        if (fetched == null)
        {
            NavigationManager.NavigateTo("/events");
            return;
        }

        _event = fetched;
        var allItems = (await InventoryItemRepository.GetAllAsync()).ToList();

        var eventItems = _event.EventInventoryItems?.ToList() ?? new List<EventInventoryItem>();
        var inventoryById = allItems.ToDictionary(i => i.Id, i => i);

        _rows = new List<EventLoadingRow>();
        foreach (var ei in eventItems)
            if (inventoryById.TryGetValue(ei.InventoryItemId, out var inventoryItem))
                _rows.Add(new EventLoadingRow
                {
                    Item = inventoryItem,
                    Quantity = ei.Quantity,
                    SelectedPriority = inventoryItem.TruckLoadingPriority
                });
    }

    private async Task SavePriorities()
    {
        foreach (var row in _rows)
            if (row.Item.TruckLoadingPriority != row.SelectedPriority)
            {
                row.Item.TruckLoadingPriority = row.SelectedPriority;
                await InventoryItemRepository.UpdateAsync(row.Item);
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