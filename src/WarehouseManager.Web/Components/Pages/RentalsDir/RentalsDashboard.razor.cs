using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

public partial class RentalsDashboard
{
    private IEnumerable<Rental>? _rentals;
    private string _search = string.Empty;
    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IEnumerable<Rental> _filtered => string.IsNullOrWhiteSpace(_search)
        ? _rentals ?? Enumerable.Empty<Rental>()
        : (_rentals ?? Enumerable.Empty<Rental>())
        .Where(r => (r.ClientName ?? string.Empty).Contains(_search, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        _rentals = await RentalRepository.GetAllAsync();
    }

    private async Task ConfirmAndDelete(int id)
    {
        try
        {
            var confirmed = await JS.InvokeAsync<bool>("confirm", "Delete this rental? This cannot be undone.");
            if (!confirmed) return;
            await RentalRepository.DeleteAsync(id);
            _rentals = await RentalRepository.GetAllAsync();
        }
        catch
        {
            // swallow errors for now; could surface a toast
        }
    }

    private string GetStatusBadge(Rental r)
    {
        return r.Status switch
        {
            RentalOrderStatus.Overdue => "bg-danger",
            RentalOrderStatus.Rented => "bg-warning text-dark",
            RentalOrderStatus.Returned => "bg-success",
            _ => "bg-secondary"
        };
    }

    private string GetPaymentBadge(PaymentStatus s)
    {
        return s switch
        {
            PaymentStatus.Paid => "bg-success",
            PaymentStatus.Invoice => "bg-info text-dark",
            _ => "bg-secondary"
        };
    }
}