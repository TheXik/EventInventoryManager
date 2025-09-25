using Microsoft.AspNetCore.Components;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

public partial class RentalDetails
{
    private Rental? _rental;
    [Parameter] public int RentalId { get; set; }

    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        _rental = await RentalRepository.GetByIdAsync(RentalId);
        // If this is a draft, automatically redirect to the creation workflow to continue editing
        if (_rental != null && _rental.Status == RentalOrderStatus.Draft)
        {
            Nav.NavigateTo($"/rentals/new/{_rental.RentalId}");
            return;
        }
    }

    private async Task SavePaymentStatus()
    {
        if (_rental == null) return;
        await RentalRepository.UpdateAsync(_rental);
        // optional: reload to ensure latest state
        _rental = await RentalRepository.GetByIdAsync(RentalId);
    }

    private string Eur(decimal value)
    {
        return $"€{value:N2}";
    }
}