using Microsoft.AspNetCore.Components;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

public partial class RentalCreationWorkflow
{
    [Parameter] public int? RentalId { get; set; }

    private readonly List<RentalRow> _lines = new();

    private readonly Rental _model = new()
        { RentalDate = DateTime.Today, ExpectedReturnDate = DateTime.Today.AddDays(1) };

    private List<InventoryItem> _all = new();
    private List<InventoryItem> _available = new();
    private Rental? _draft;
    private string _search = string.Empty;

    private int _step = 1;
    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private IRentalItemRepository RentalItemRepository { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private int _days => Math.Max(1, (int)Math.Ceiling((_model.ExpectedReturnDate - _model.RentalDate).TotalDays));

    private decimal Subtotal => _lines.Sum(l => l.PricePerDay * l.Quantity * _days);
    private decimal Discount => Math.Round(Subtotal * (_model.DiscountPercentage / 100m), 2);
    private decimal Total => Subtotal - Discount;

    protected override async Task OnInitializedAsync()
    {
        _all = (await InventoryRepository.GetAllAsync()).ToList();

        if (RentalId.HasValue)
        {
            await LoadDraftAsync(RentalId.Value);
        }

        Filter();
    }

    private async Task LoadDraftAsync(int id)
    {
        var existing = await RentalRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return;
        }

        // Only allow editing drafts here; if already rented, redirect to details
        if (existing.Status != RentalOrderStatus.Draft)
        {
            Nav.NavigateTo($"/rentals/{existing.RentalId}");
            return;
        }

        _draft = existing;
        _model.ClientName = existing.ClientName;
        _model.ContactInfo = existing.ContactInfo;
        _model.RentalDate = existing.RentalDate;
        _model.ExpectedReturnDate = existing.ExpectedReturnDate;
        _model.DiscountPercentage = existing.DiscountPercentage;

        var rentalItems = (await RentalItemRepository.GetByRentalIdAsync(existing.RentalId)).ToList();
        _draft.RentalItems = rentalItems;

        _lines.Clear();
        foreach (var ri in rentalItems)
        {
            var inv = await InventoryRepository.GetByIdAsync(ri.InventoryItemId);
            if (inv == null) continue;

            _lines.Add(new RentalRow
            {
                Item = inv,
                Quantity = ri.QuantityRented,
                PricePerDay = ri.PricePerDayAtTimeOfRental
            });
        }

        _step = 2;
    }

    private async Task GoToStep2()
    {
        if (_draft == null)
        {
            _draft = await RentalRepository.AddAsync(new Rental
            {
                ClientName = _model.ClientName,
                ContactInfo = _model.ContactInfo ?? string.Empty,
                RentalDate = _model.RentalDate,
                ExpectedReturnDate = _model.ExpectedReturnDate,
                DiscountPercentage = _model.DiscountPercentage,
                Status = RentalOrderStatus.Draft
            });
        }
        else
        {
            _draft.ClientName = _model.ClientName;
            _draft.ContactInfo = _model.ContactInfo ?? string.Empty;
            _draft.RentalDate = _model.RentalDate;
            _draft.ExpectedReturnDate = _model.ExpectedReturnDate;
            _draft.DiscountPercentage = _model.DiscountPercentage;
            await RentalRepository.UpdateAsync(_draft);
        }

        // Ensure the URL contains the draft ID so the user can resume later if they close after Step 1
        if (_draft != null)
        {
            Nav.NavigateTo($"/rentals/new/{_draft.RentalId}", true);
        }

        _step = 2;
    }

    private void Filter()
    {
        _available = _all
            .Where(i => (i.Name ?? string.Empty).Contains(_search, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task AddItem(InventoryItem item)
    {
        // Decrease availability immediately
        if (item.AvailableQuantity <= 0 || _draft == null) return;
        item.AvailableQuantity -= 1;
        item.UpdateAvailabilityStatus();
        await InventoryRepository.UpdateAsync(item);

        var existing = _lines.FirstOrDefault(l => l.Item.Id == item.Id);
        if (existing == null)
        {
            existing = new RentalRow
            {
                Item = item,
                Quantity = 1,
                PricePerDay = EffectivePrice(item)
            };
            _lines.Add(existing);

            // persist RentalItem
            var created = new RentalItem
            {
                RentalId = _draft.RentalId,
                InventoryItemId = item.Id,
                QuantityRented = 1,
                PricePerDayAtTimeOfRental = EffectivePrice(item)
            };
            await RentalItemRepository.AddAsync(created);
            // keep in-memory draft consistent for later updates
            _draft.RentalItems ??= new List<RentalItem>();
            _draft.RentalItems.Add(created);
        }
        else
        {
            existing.Quantity += 1;
            // update rental item quantity
            var ri = _draft.RentalItems.FirstOrDefault(x => x.InventoryItemId == item.Id);
            if (ri != null)
            {
                ri.QuantityRented = existing.Quantity;
                await RentalRepository.UpdateAsync(_draft);
            }
        }

        // Ensure rental status reflects that items are in use
        // Keep rental as Draft until final confirmation to allow resuming later
        // Do not auto-switch to Rented here

        Filter();
        StateHasChanged();
    }

    private async Task RemoveLine(RentalRow line)
    {
        if (_draft == null) return;
        // Return the currently reserved quantity back to availability
        var item = line.Item;
        item.AvailableQuantity += line.Quantity;
        item.UpdateAvailabilityStatus();
        await InventoryRepository.UpdateAsync(item);

        _lines.Remove(line);

        // also remove from DB rental items
        var toRemove = _draft.RentalItems.FirstOrDefault(x => x.InventoryItemId == item.Id);
        if (toRemove != null)
        {
            toRemove.QuantityRented = 0;
            await RentalRepository.UpdateAsync(_draft);
        }

        Filter();
        StateHasChanged();
    }

    private async Task OnQtyChange(RentalRow line, ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var newQty)) return;
        newQty = Math.Max(1, newQty);
        var delta = newQty - line.Quantity;
        if (delta == 0)
        {
            StateHasChanged();
            return;
        }

        // Adjust inventory
        if (delta > 0)
        {
            // Need more units; ensure available
            if (line.Item.AvailableQuantity < delta)
            {
                StateHasChanged();
                return;
            }

            line.Item.AvailableQuantity -= delta;
        }
        else
        {
            // releasing some units
            line.Item.AvailableQuantity += -delta;
        }

        line.Item.UpdateAvailabilityStatus();
        await InventoryRepository.UpdateAsync(line.Item);

        line.Quantity = newQty;

        if (_draft != null)
        {
            var ri = _draft.RentalItems.FirstOrDefault(x => x.InventoryItemId == line.Item.Id);
            if (ri != null)
            {
                ri.QuantityRented = line.Quantity;
                await RentalRepository.UpdateAsync(_draft);
            }
        }

        // Ensure UI totals refresh immediately
        StateHasChanged();
    }

    private decimal EffectivePrice(InventoryItem item)
    {
        return item.RentalPricePerDay > 0 ? item.RentalPricePerDay : item.RentalPricePerDay;
    }

    private string Eur(decimal value)
    {
        return $"€{value:N2}";
    }

    private void GoToStep3()
    {
        _step = 3;
    }

    private async Task ConfirmDispatch()
    {
        if (_draft == null) return;
        _draft.Status = RentalOrderStatus.Rented;
        _draft.RentalItems = _lines.Select(l => new RentalItem
        {
            RentalId = _draft.RentalId,
            InventoryItemId = l.Item.Id,
            QuantityRented = l.Quantity,
            QuantityReturned = 0,
            PricePerDayAtTimeOfRental = l.PricePerDay
        }).ToList();
        await RentalRepository.UpdateAsync(_draft);

        foreach (var line in _lines)
        {
            line.Item.RentalStatus = RentalStatus.Rented;
            await InventoryRepository.UpdateAsync(line.Item);
        }

        Nav.NavigateTo($"/rentals/{_draft.RentalId}");
    }
}