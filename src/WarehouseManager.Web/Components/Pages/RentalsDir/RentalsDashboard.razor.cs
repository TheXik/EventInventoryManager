using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

/// <summary>
/// Dashboard component for managing rental orders
/// Displays all rentals with search functionality and status management
/// </summary>
public partial class RentalsDashboard
{
    // Data and UI State
    private IEnumerable<Rental>? _rentals;
    private string _search = string.Empty;
    private string? _errorMessage;
    private bool _isLoading = true;

    // Dependency Injection
    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<RentalsDashboard> Logger { get; set; } = default!;

    /// <summary>
    /// Computed property that filters rentals based on search query.
    /// Applies case-insensitive search to client names.
    /// </summary>
    private IEnumerable<Rental> _filtered => string.IsNullOrWhiteSpace(_search)
        ? _rentals ?? Enumerable.Empty<Rental>()
        : (_rentals ?? Enumerable.Empty<Rental>())
            .Where(r => (r.ClientName ?? string.Empty).Contains(_search, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.ExpectedReturnDate);

    /// <summary>
    /// Initializes the component by loading all rental data.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            _rentals = await RentalRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load rentals: {ex.Message}";
            Logger.LogError(ex, "Error loading rentals");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Confirms and deletes a rental after user confirmation
    /// </summary>
    /// <param name="id">The rental ID to delete</param>
    private async Task ConfirmAndDelete(int id)
    {
        try
        {
            var confirmed = await JS.InvokeAsync<bool>("confirm", 
                "Delete this rental? This cannot be undone.");
            if (!confirmed) return;

            await RentalRepository.DeleteAsync(id);
            _rentals = await RentalRepository.GetAllAsync();
            _errorMessage = null;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to delete rental: {ex.Message}";
            Logger.LogError(ex, "Error deleting rental {RentalId}", id);
        }
    }

    /// <summary>
    /// Returns the appropriate CSS class for rental status badges
    /// </summary>
    /// <param name="r">The rental to get status for</param>
    /// <returns>Bootstrap badge CSS class</returns>
    private static string GetStatusBadge(Rental r)
    {
        return r.Status switch
        {
            RentalOrderStatus.Overdue => "bg-danger",
            RentalOrderStatus.Rented => "bg-warning text-dark",
            RentalOrderStatus.Returned => "bg-success",
            RentalOrderStatus.Draft => "bg-light text-dark",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// Returns the appropriate CSS class for payment status badges
    /// </summary>
    /// <param name="s">The payment status</param>
    /// <returns>Bootstrap badge CSS class</returns>
    private static string GetPaymentBadge(PaymentStatus s)
    {
        return s switch
        {
            PaymentStatus.Paid => "bg-success",
            PaymentStatus.Invoice => "bg-info text-dark",
            PaymentStatus.Unpaid => "bg-warning text-dark",
            _ => "bg-secondary"
        };
    }
}