using Microsoft.AspNetCore.Components;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;

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
    }

    private string Eur(decimal value)
    {
        return $"€{value:N2}";
    }
}