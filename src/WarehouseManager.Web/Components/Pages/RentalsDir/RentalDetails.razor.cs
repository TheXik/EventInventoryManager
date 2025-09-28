using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

/// <summary>
/// Component for displaying detailed information about a specific rental
/// Handles rental status updates and payment status management
/// </summary>
public partial class RentalDetails
{
    // Data and UI State
    private Rental? _rental;
    private string? _errorMessage;
    private bool _isLoading = true;

    // Parameters
    [Parameter] public int RentalId { get; set; }

    // Dependency Injection
    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ILogger<RentalDetails> Logger { get; set; } = default!;

    /// <summary>
    /// Loads rental data when component parameters are set
    /// Redirects draft rentals to the creation workflow
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            _rental = await RentalRepository.GetByIdAsync(RentalId);
            
            if (_rental == null)
            {
                _errorMessage = "Rental not found.";
                return;
            }

            // If this is a draft, automatically redirect to the creation workflow to continue editing
            if (_rental.Status == RentalOrderStatus.Draft)
            {
                Nav.NavigateTo($"/rentals/new/{_rental.RentalId}");
                return;
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load rental: {ex.Message}";
            Logger.LogError(ex, "Error loading rental {RentalId}", RentalId);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Saves the updated payment status to the database
    /// </summary>
    private async Task SavePaymentStatus()
    {
        try
        {
            if (_rental == null) return;

            await RentalRepository.UpdateAsync(_rental);
            
            // Reload to ensure latest state
            _rental = await RentalRepository.GetByIdAsync(RentalId);
            _errorMessage = null;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save payment status: {ex.Message}";
            Logger.LogError(ex, "Error saving payment status for rental {RentalId}", RentalId);
        }
    }

    /// <summary>
    /// Formats a decimal value as Euro currency
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <returns>Formatted currency string</returns>
    private static string Eur(decimal value)
    {
        return $"€{value:N2}";
    }
}