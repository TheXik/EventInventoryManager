using Microsoft.AspNetCore.Components;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

public partial class RentalCreationWorkflow
{
    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private IRentalItemRepository RentalItemRepository { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private int _step = 1;
    private Rental _model = new() { RentalDate = DateTime.Today, ExpectedReturnDate = DateTime.Today.AddDays(1) };
    private Rental? _draft;

    private List<InventoryItem> _all = new();
    private List<InventoryItem> _available = new();
    private string _search = string.Empty;

    private List<RentalRow> _lines = new();

    private int _days => Math.Max(1, (int)Math.Ceiling((_model.ExpectedReturnDate - _model.RentalDate).TotalDays));

    protected override async Task OnInitializedAsync()
    {
        _all = (await InventoryRepository.GetAllAsync()).ToList();
        Filter();
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
        // Mark item as in rental use
        item.RentalStatus = RentalStatus.Rented;
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
        if (_draft != null && _draft.Status != RentalOrderStatus.Rented && _lines.Any(l => l.Quantity > 0))
        {
            _draft.Status = RentalOrderStatus.Rented;
            await RentalRepository.UpdateAsync(_draft);
        }

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
        // If all units are free, mark as available; otherwise still rented
        item.RentalStatus = (item.AvailableQuantity < item.TotalQuantity)
            ? RentalStatus.Rented
            : RentalStatus.Available;
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
                StateHasChanged(); return;
            }
            line.Item.AvailableQuantity -= delta;
        }
        else
        {
            // releasing some units
            line.Item.AvailableQuantity += (-delta);
        }
        line.Item.UpdateAvailabilityStatus();
        // Update rental status based on allocation
        line.Item.RentalStatus = (line.Item.AvailableQuantity < line.Item.TotalQuantity)
            ? RentalStatus.Rented
            : RentalStatus.Available;
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

    private decimal Subtotal => _lines.Sum(l => l.PricePerDay * l.Quantity * _days);
    private decimal Discount => Math.Round(Subtotal * (_model.DiscountPercentage / 100m), 2);
    private decimal Total => Subtotal - Discount;

    private decimal EffectivePrice(InventoryItem item) => item.RentalPricePerDay > 0 ? item.RentalPricePerDay : (decimal)item.RentalPricePerDay;

    private string Eur(decimal value) => $"€{value:N2}";

    private void GoToStep3() => _step = 3;

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

        Nav.NavigateTo($"/rentals/{_draft.RentalId}");
    }

  
}

