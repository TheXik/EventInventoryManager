using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

/// <summary>
/// Component for processing rental returns
/// Handles item returns, damage assessment, and inventory updates
/// </summary>
public partial class RentalReturn
{
    // Data and UI State
    private List<ReturnLine> _lines = new();
    private Rental? _rental;
    private string? _errorMessage;
    private bool _isLoading = true;

    // Parameters
    [Parameter] public int RentalId { get; set; }

    // Dependency Injection
    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ILogger<RentalReturn> Logger { get; set; } = default!;

    /// <summary>
    /// Loads rental data and initializes return lines when component parameters are set.
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

            _lines = _rental.RentalItems?.Select(ri => new ReturnLine
            {
                RentalItem = ri,
                ReturnQty = ri.QuantityReturned
            }).ToList() ?? new List<ReturnLine>();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load rental: {ex.Message}";
            Logger.LogError(ex, "Error loading rental {RentalId} for return", RentalId);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }


    /// <summary>
    /// Processes the rental return, updating inventory and rental status
    /// </summary>
    private async Task ConfirmReturn()
    {
        try
        {
            if (_rental == null) return;

            _isLoading = true;
            _errorMessage = null;

            // Process each return line
            foreach (var line in _lines)
            {
                var ri = line.RentalItem;
                var deltaReturned = line.ReturnQty - ri.QuantityReturned;
                ri.QuantityReturned = line.ReturnQty;

                var item = await InventoryRepository.GetByIdAsync(ri.InventoryItemId);
                if (item != null)
                {
                    // Update availability if items are being returned
                    if (deltaReturned > 0)
                    {
                        item.AvailableQuantity += deltaReturned;
                        item.UpdateAvailabilityStatus();
                    }

                    // Handle damaged items
                    if (line.Damaged)
                    {
                        item.Condition = Condition.Damaged;
                        item.RentalStatus = RentalStatus.NotInRentalUse; // damaged items are temporarily not rentable
                        if (!string.IsNullOrWhiteSpace(line.Notes)) 
                            item.ConditionDescription = line.Notes;
                    }

                    // Update item after changes to availability or condition
                    if (deltaReturned > 0 || line.Damaged) 
                        await InventoryRepository.UpdateAsync(item);
                }
            }

            // Check if all items are returned and close rental if so
            var allReturned = _rental.RentalItems?.All(x => x.QuantityReturned >= x.QuantityRented) ?? false;
            if (allReturned)
            {
                _rental.Status = RentalOrderStatus.Returned;
                _rental.ActualReturnDate = DateTime.Now;
            }

            await RentalRepository.UpdateAsync(_rental);
            Nav.NavigateTo($"/rentals/{_rental.RentalId}");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to process return: {ex.Message}";
            Logger.LogError(ex, "Error processing return for rental {RentalId}", RentalId);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Represents a line item in the return process.
    /// Contains rental item information and return details.
    /// </summary>
    private class ReturnLine
    {
        /// <summary>
        /// The rental item being returned.
        /// </summary>
        public RentalItem RentalItem { get; set; } = default!;
        
        /// <summary>
        /// Quantity being returned.
        /// </summary>
        public int ReturnQty { get; set; }
        
        /// <summary>
        /// Whether the item is damaged.
        /// </summary>
        public bool Damaged { get; set; }
        
        /// <summary>
        /// Notes about the return condition.
        /// </summary>
        public string? Notes { get; set; }
    }
}