using Microsoft.AspNetCore.Components;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

public partial class RentalDetails
{
    [Parameter] public int RentalId { get; set; }

    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private Rental? _rental;

    protected override async Task OnParametersSetAsync()
    {
        _rental = await RentalRepository.GetByIdAsync(RentalId);
    }

    private string Eur(decimal value) => $"€{value:N2}";
}
