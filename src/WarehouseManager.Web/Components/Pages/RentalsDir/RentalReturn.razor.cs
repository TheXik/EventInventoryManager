using Microsoft.AspNetCore.Components;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

public partial class RentalReturn
{
    [Parameter] public int RentalId { get; set; }

    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private Rental? _rental;

    private List<ReturnLine> _lines = new();

    protected override async Task OnParametersSetAsync()
    {
        _rental = await RentalRepository.GetByIdAsync(RentalId);
        _lines = _rental?.RentalItems.Select(ri => new ReturnLine
        {
            RentalItem = ri,
            ReturnQty = ri.QuantityReturned
        }).ToList() ?? new List<ReturnLine>();
    }


    private async Task ConfirmReturn()
    {
        if (_rental == null) return;
        foreach (var line in _lines)
        {
            var ri = line.RentalItem;
            var deltaReturned = line.ReturnQty - ri.QuantityReturned;
            ri.QuantityReturned = line.ReturnQty;

            var item = await InventoryRepository.GetByIdAsync(ri.InventoryItemId);
            if (item != null)
            {
                if (deltaReturned > 0)
                {
                    item.AvailableQuantity += deltaReturned;
                    item.UpdateAvailabilityStatus();
                }

                if (line.Damaged)
                {
                    item.Condition = Condition.Damaged;
                    item.RentalStatus = RentalStatus.Damaged;
                    if (!string.IsNullOrWhiteSpace(line.Notes))
                    {
                        item.ConditionDescription = line.Notes;
                    }
                }
                else
                {
                    // If not damaged, set rental status based on whether any units remain allocated
                    item.RentalStatus = (item.AvailableQuantity < item.TotalQuantity)
                        ? RentalStatus.Rented
                        : RentalStatus.Available;
                }

                if (deltaReturned > 0 || line.Damaged)
                {
                    await InventoryRepository.UpdateAsync(item);
                }
            }
        }

        // If everything returned, close rental
        var allReturned = _rental.RentalItems.All(x => x.QuantityReturned >= x.QuantityRented);
        if (allReturned)
        {
            _rental.Status = WarehouseManager.Core.Enums.RentalOrderStatus.Returned;
            _rental.ActualReturnDate = DateTime.Now;
        }

        await RentalRepository.UpdateAsync(_rental);
        Nav.NavigateTo($"/rentals/{_rental.RentalId}");
    }

    private class ReturnLine
    {
        public RentalItem RentalItem { get; set; } = default!;
        public int ReturnQty { get; set; }
        public bool Damaged { get; set; }
        public string? Notes { get; set; }
    }
}
