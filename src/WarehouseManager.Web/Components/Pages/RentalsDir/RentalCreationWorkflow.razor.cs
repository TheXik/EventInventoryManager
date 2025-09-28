using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages.RentalsDir;

/// <summary>
/// Multi-step rental creation workflow component
/// Handles the complete rental process from client information to final dispatch.
/// </summary>
public partial class RentalCreationWorkflow
{
    [Parameter] public int? RentalId { get; set; }

    // UI State Management
    private readonly List<RentalRow> _lines = new(); // List of rental items
    private readonly Rental _model = new() // Rental model
    { 
        RentalDate = DateTime.Today, 
        ExpectedReturnDate = DateTime.Today.AddDays(1) 
    };
    private List<InventoryItem> _all = new(); // All inventory items
    private List<InventoryItem> _available = new(); // Available inventory items
    private Rental? _draft; // Draft rental
    private string _search = string.Empty; // Search query
    private int _step = 1; // Current step
    private string? _errorMessage; 
    private bool _isLoading = false;

    // Dependency Injection
    [Inject] private IRentalRepository RentalRepository { get; set; } = default!;
    [Inject] private IRentalItemRepository RentalItemRepository { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryRepository { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ILogger<RentalCreationWorkflow> Logger { get; set; } = default!;

    /// <summary>
    /// Calculates the number of rental days between start and end date
    /// Ensures minimum of 1 day rental period. TODO maybe should be also an option to rent for a number of hours just
    /// </summary>
    private int _days => Math.Max(1, (int)Math.Ceiling((_model.ExpectedReturnDate - _model.RentalDate).TotalDays));

    /// <summary>
    /// Calculates subtotal price for all selected items
    /// </summary>
    private decimal Subtotal => _lines.Sum(l => l.PricePerDay * l.Quantity * _days);
    
    /// <summary>
    /// Calculates discount amount based on percentage 
    /// </summary>
    private decimal Discount => Math.Round(Subtotal * (_model.DiscountPercentage / 100m), 2);
    
    /// <summary>
    /// Calculates final total after discount
    /// </summary>
    private decimal Total => Subtotal - Discount;

    /// <summary>
    /// Initializes the component by loading inventory data and optionally loading an existing draft
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            // Load all inventory items
            _all = (await InventoryRepository.GetAllAsync()).ToList();

            // Load existing draft if editing
            if (RentalId.HasValue)
            {
                await LoadDraftAsync(RentalId.Value);
            }

            Filter();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to initialize rental workflow: {ex.Message}";
            Logger.LogError(ex, "Error initializing rental creation workflow");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Loads an existing draft rental for editing
    /// </summary>
    /// <param name="id">The rental ID to load</param>
    private async Task LoadDraftAsync(int id)
    {
        try
        {
            var existing = await RentalRepository.GetByIdAsync(id);
            if (existing == null)
            {
                _errorMessage = "Rental not found.";
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

            // Load rental items and populate lines
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
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load draft rental: {ex.Message}";
            Logger.LogError(ex, "Error loading draft rental {RentalId}", id);
        }
    }

    /// <summary>
    /// Saves client information and advances to step 2 (item selection)
    /// Creates a new draft rental or updates existing one
    /// </summary>
    private async Task GoToStep2()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            if (_draft == null)
            {
                // Create new draft rental
                _draft = await RentalRepository.AddAsync(new Rental
                {
                    ClientName = _model.ClientName?.Trim(),
                    ContactInfo = _model.ContactInfo?.Trim() ?? string.Empty,
                    RentalDate = _model.RentalDate,
                    ExpectedReturnDate = _model.ExpectedReturnDate,
                    DiscountPercentage = _model.DiscountPercentage,
                    Status = RentalOrderStatus.Draft
                });
            }
            else
            {
                // Update existing draft
                _draft.ClientName = _model.ClientName?.Trim();
                _draft.ContactInfo = _model.ContactInfo?.Trim() ?? string.Empty;
                _draft.RentalDate = _model.RentalDate;
                _draft.ExpectedReturnDate = _model.ExpectedReturnDate;
                _draft.DiscountPercentage = _model.DiscountPercentage;
                await RentalRepository.UpdateAsync(_draft);
            }

            // Update URL to include draft ID for resumability
            if (_draft != null)
            {
                Nav.NavigateTo($"/rentals/new/{_draft.RentalId}", true);
            }

            _step = 2;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save rental information: {ex.Message}";
            Logger.LogError(ex, "Error saving rental information");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Filters available inventory items based on search query
    /// </summary>
    private void Filter()
    {
        _available = _all
            .Where(i => string.IsNullOrWhiteSpace(_search) || 
                       (i.Name ?? string.Empty).Contains(_search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Name)
            .ToList();
    }

    /// <summary>
    /// Adds an inventory item to the rental
    /// Updates availability and creates/updates rental items
    /// </summary>
    /// <param name="item">The inventory item to add</param>
    private async Task AddItem(InventoryItem item)
    {
        try
        {
            if (item.AvailableQuantity <= 0 || _draft == null) return;

            // Decrease availability immediately
            item.AvailableQuantity -= 1;
            item.UpdateAvailabilityStatus();
            await InventoryRepository.UpdateAsync(item);

            var existing = _lines.FirstOrDefault(l => l.Item.Id == item.Id);
            if (existing == null)
            {
                // Create new rental line
                existing = new RentalRow
                {
                    Item = item,
                    Quantity = 1,
                    PricePerDay = EffectivePrice(item)
                };
                _lines.Add(existing);

                // Persist to database
                var created = new RentalItem
                {
                    RentalId = _draft.RentalId,
                    InventoryItemId = item.Id,
                    QuantityRented = 1,
                    PricePerDayAtTimeOfRental = EffectivePrice(item)
                };
                await RentalItemRepository.AddAsync(created);
                
                // Keep in-memory draft consistent
                _draft.RentalItems ??= new List<RentalItem>();
                _draft.RentalItems.Add(created);
            }
            else
            {
                // Update existing line
                existing.Quantity += 1;
                
                // Update database record
                var ri = _draft.RentalItems.FirstOrDefault(x => x.InventoryItemId == item.Id);
                if (ri != null)
                {
                    ri.QuantityRented = existing.Quantity;
                    await RentalRepository.UpdateAsync(_draft);
                }
            }

            Filter();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to add item: {ex.Message}";
            Logger.LogError(ex, "Error adding item {ItemId} to rental", item.Id);
        }
    }

    /// <summary>
    /// Removes a rental line and returns items to available inventory
    /// </summary>
    /// <param name="line">The rental line to remove</param>
    private async Task RemoveLine(RentalRow line)
    {
        try
        {
            if (_draft == null) return;

            // Return reserved quantity back to availability
            var item = line.Item;
            item.AvailableQuantity += line.Quantity;
            item.UpdateAvailabilityStatus();
            await InventoryRepository.UpdateAsync(item);

            _lines.Remove(line);

            // Update database record
            var toRemove = _draft.RentalItems.FirstOrDefault(x => x.InventoryItemId == item.Id);
            if (toRemove != null)
            {
                toRemove.QuantityRented = 0;
                await RentalRepository.UpdateAsync(_draft);
            }

            Filter();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to remove item: {ex.Message}";
            Logger.LogError(ex, "Error removing item {ItemId} from rental", line.Item.Id);
        }
    }

    /// <summary>
    /// Handles quantity changes for rental lines
    /// Updates inventory availability and database records
    /// </summary>
    /// <param name="line">The rental line being modified</param>
    /// <param name="e">The change event arguments</param>
    private async Task OnQtyChange(RentalRow line, ChangeEventArgs e)
    {
        try
        {
            if (!int.TryParse(e.Value?.ToString(), out var newQty)) return;
            newQty = Math.Max(1, newQty);
            var delta = newQty - line.Quantity;
            
            if (delta == 0)
            {
                StateHasChanged();
                return;
            }

            // Adjust inventory availability
            if (delta > 0)
            {
                // Need more units; check availability
                if (line.Item.AvailableQuantity < delta)
                {
                    _errorMessage = $"Not enough available units. Only {line.Item.AvailableQuantity} available.";
                    StateHasChanged();
                    return;
                }
                line.Item.AvailableQuantity -= delta;
            }
            else
            {
                // Releasing units back to inventory
                line.Item.AvailableQuantity += Math.Abs(delta);
            }

            line.Item.UpdateAvailabilityStatus();
            await InventoryRepository.UpdateAsync(line.Item);
            line.Quantity = newQty;

            // Update database record
            if (_draft != null)
            {
                var ri = _draft.RentalItems.FirstOrDefault(x => x.InventoryItemId == line.Item.Id);
                if (ri != null)
                {
                    ri.QuantityRented = line.Quantity;
                    await RentalRepository.UpdateAsync(_draft);
                }
            }

            _errorMessage = null;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to update quantity: {ex.Message}";
            Logger.LogError(ex, "Error updating quantity for item {ItemId}", line.Item.Id);
        }
    }

    /// <summary>
    /// Calculates the effective rental price for an item
    /// </summary>
    /// <param name="item">The inventory item</param>
    /// <returns>The effective daily rental price</returns>
    private static decimal EffectivePrice(InventoryItem item)
    {
        return item.RentalPricePerDay > 0 ? item.RentalPricePerDay : 0m;
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

    /// <summary>
    /// Advances to step 3 (payment and confirmation)
    /// </summary>
    private void GoToStep3()
    {
        _step = 3;
    }

    /// <summary>
    /// Confirms the rental dispatch and finalizes the rental process
    /// </summary>
    private async Task ConfirmDispatch()
    {
        try
        {
            if (_draft == null || !_lines.Any())
            {
                _errorMessage = "Cannot dispatch rental without items.";
                return;
            }

            _isLoading = true;
            _errorMessage = null;

            // Update rental status to Rented
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

            // Update item rental statuses
            foreach (var line in _lines)
            {
                line.Item.RentalStatus = RentalStatus.Rented;
                await InventoryRepository.UpdateAsync(line.Item);
            }

            // Navigate to rental details
            Nav.NavigateTo($"/rentals/{_draft.RentalId}");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to dispatch rental: {ex.Message}";
            Logger.LogError(ex, "Error dispatching rental {RentalId}", _draft?.RentalId);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}