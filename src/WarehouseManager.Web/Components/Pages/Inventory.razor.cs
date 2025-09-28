using System.ComponentModel.DataAnnotations;
using Microsoft.JSInterop;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages;

/// <summary>
/// View model for inventory item operations (add/edit forms)
/// Keeps UI concerns separate from domain entities
/// </summary>
internal class InventoryItemViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Name is too long.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description is too long.")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
    public int Quantity { get; set; }

    public int CategoryId { get; set; }

    // Only used if user creates a new category
    public string? NewCategoryName { get; set; }

    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.Available;

    [Range(0, int.MaxValue, ErrorMessage = "Weight must be a non-negative number.")]
    public int Weight { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Dimensions must be non-negative.")]
    public int Height { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Dimensions must be non-negative.")]
    public int Width { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Dimensions must be non-negative.")]
    public int Length { get; set; }

    public TruckLoadingPriority? TruckLoadingPriority { get; set; }
    public RentalStatus RentalStatus { get; set; } = RentalStatus.NotInRentalUse;
    public DateTime? RentalDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
    public decimal RentalPricePerDay { get; set; }

    public string? RentalDescription { get; set; }
    public Condition? Condition { get; set; }
    public string? ConditionDescription { get; set; }
}

/// <summary>
/// View model for inventory filtering functionality.
/// </summary>
internal class InventoryFilterViewModel
{
    public HashSet<int> SelectedCategoryIds { get; set; } = new();
    public HashSet<AvailabilityStatus> SelectedAvailabilityStatuses { get; set; } = new();
    public HashSet<Condition> SelectedConditions { get; set; } = new();
}

/// <summary>
/// Inventory management page component. Handles basic operations for inventory items
/// filtering, searching, and category management
/// </summary>
public partial class Inventory
{
    // Data collections
    private List<InventoryItem>? _allItems;
    private List<ItemCategory> _categories = new();
    private string? _errorMessage;
    private bool _isLoading = true;

    // UI state management
    private InventoryFilterViewModel _filters = new();
    private bool _isAddModalVisible;
    private bool _isFilterModalVisible;
    private bool _isItemDetailsModalVisible;
    private bool _isItemEditModalVisible; 
    private string _searchQuery = ""; // Search query for filtering items
    private InventoryItem? _selectedItem;  // Selected item for details and edit
    private InventoryItemViewModel _viewModel = new(); // View model for add/edit forms

    private IEnumerable<InventoryItem> FilteredItems => 
        _allItems?
            .Where(item =>
                // Search Filter
                (string.IsNullOrWhiteSpace(_searchQuery) ||
                 item.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                 (item.Description ?? "").Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)) &&

                // Category Filter
                (!_filters.SelectedCategoryIds.Any() ||
                 _filters.SelectedCategoryIds.Contains(item.CategoryId)) &&

                // Availability Filter
                (!_filters.SelectedAvailabilityStatuses.Any() ||
                 _filters.SelectedAvailabilityStatuses.Contains(item.AvailabilityStatus)) &&

                // Condition Filter
                (!_filters.SelectedConditions.Any() ||
                 (item.Condition.HasValue && _filters.SelectedConditions.Contains(item.Condition.Value)))
            )
            // If _allItems is null retrun empty list
            ?? Enumerable.Empty<InventoryItem>();

    /// <summary>
    /// Loads inventory items and categories from the database on component initialization.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    /// <summary>
    /// Loads inventory data and categories from repositories.
    /// Manages loading state and error handling.
    /// </summary>
    private async Task LoadData()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            // Get all data from the database
            var itemsTask = InventoryRepo.GetAllAsync();
            var categoriesTask = CategoryRepo.GetAllAsync();
            await Task.WhenAll(itemsTask, categoriesTask);

            _allItems = (await itemsTask).ToList();
            _categories = (await categoriesTask).ToList();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load data: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Toggles selection state for filter checkboxes
    /// </summary>
    /// <typeparam name="T">Type of items in the HashSet</typeparam>
    /// <param name="set">The HashSet to modify</param>
    /// <param name="value">The value to toggle</param>
    private void ToggleSelection<T>(HashSet<T> set, T value)
    {
        if (set.Contains(value))
            set.Remove(value);
        else
            set.Add(value);
    }

    /// <summary>
    /// Handles form submission for adding new inventory items
    /// Creates new categories if needed and validates input data
    /// </summary>
    private async Task HandleValidSubmit()
    {
        try
        {
            // Handle new category creation
            if (_viewModel.CategoryId == -1)
            {
                if (string.IsNullOrWhiteSpace(_viewModel.NewCategoryName))
                {
                    _errorMessage = "Please provide a name for the new category.";
                    return;
                }

                var newCategory = new ItemCategory { Name = _viewModel.NewCategoryName.Trim() };
                var createdCategory = await CategoryRepo.AddAsync(newCategory);
                _viewModel.CategoryId = createdCategory.Id;
                
                _categories = (await CategoryRepo.GetAllAsync()).ToList();
            }
            // Create new inventory item
            var newItem = new InventoryItem
            {
                Name = _viewModel.Name.Trim(),
                Description = _viewModel.Description?.Trim(),
                AvailableQuantity = _viewModel.Quantity,
                TotalQuantity = _viewModel.Quantity,
                CategoryId = _viewModel.CategoryId,
                AvailabilityStatus = _viewModel.AvailabilityStatus,
                Weight = _viewModel.Weight,
                Height = _viewModel.Height,
                Width = _viewModel.Width,
                Length = _viewModel.Length,
                TruckLoadingPriority = _viewModel.TruckLoadingPriority,
                RentalStatus = _viewModel.RentalStatus,
                RentalDate = _viewModel.RentalDate ?? DateTime.UtcNow,
                RentalPricePerDay = _viewModel.RentalPricePerDay,
                Condition = _viewModel.Condition,
                ConditionDescription = _viewModel.ConditionDescription?.Trim(),
                Category = null!
            };

            await InventoryRepo.AddAsync(newItem);
            await LoadData();
            _errorMessage = null;
            CloseAddModal();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to add item: {ex.Message}";
        }
    }

    /// <summary>
    /// Handles form submission for updating existing inventory items.
    /// Maps view model data back to entity and saves changes.
    /// </summary>
    private async Task HandleUpdateSubmit()
    {
        try
        {
            // Find item to update
            var itemToUpdate = _allItems?.FirstOrDefault(i => i.Id == _viewModel.Id);
            
            // If item is not found show error message
            if (itemToUpdate is null)
            {
                _errorMessage = "The item you are trying to update was not found.";
                return;
            }

            itemToUpdate.Name = _viewModel.Name.Trim();
            itemToUpdate.Description = _viewModel.Description?.Trim();
            itemToUpdate.TotalQuantity = _viewModel.Quantity;
            itemToUpdate.CategoryId = _viewModel.CategoryId;
            itemToUpdate.AvailabilityStatus = _viewModel.AvailabilityStatus;
            itemToUpdate.Weight = _viewModel.Weight;
            itemToUpdate.Height = _viewModel.Height;
            itemToUpdate.Width = _viewModel.Width;
            itemToUpdate.Length = _viewModel.Length;
            itemToUpdate.TruckLoadingPriority = _viewModel.TruckLoadingPriority;
            itemToUpdate.RentalStatus = _viewModel.RentalStatus;
            itemToUpdate.RentalPricePerDay = _viewModel.RentalPricePerDay;
            itemToUpdate.Condition = _viewModel.Condition;
            itemToUpdate.ConditionDescription = _viewModel.ConditionDescription?.Trim();

            await InventoryRepo.UpdateAsync(itemToUpdate);

            await LoadData();
            _errorMessage = null;
            CloseEditItemModal();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to update item: {ex.Message}";
        }
    }

    /// <summary>
    /// Deletes an inventory item after checking for active rental references.
    /// </summary>
    /// <param name="item">The inventory item to delete</param>
    private async Task DeleteItem(InventoryItem item)
    {
        try
        {
            // Check active rental references
            var activeRefs = await InventoryRepo.CountActiveRentalReferencesAsync(item.Id);
            if (activeRefs > 0)
            {
                var confirm = await JS.InvokeAsync<bool>("confirm",
                    $"This item is included in {activeRefs} active rental(s). If you delete it, those rental records may be affected. Are you sure you want to Continue????");
                if (!confirm) return;
            }

            // TODO later decide if we want to have a final confirmation or it will just annoy users
            // var finalConfirm = await JS.InvokeAsync<bool>("confirm",
            //     $"Are you sure you want to delete '{item.Name}'? This action cannot be undone.");
            // if (!finalConfirm) return;

            await InventoryRepo.DeleteAsync(item.Id);
            await LoadData();
            _errorMessage = null;
        }
        catch (InvalidOperationException ex)
        {
            _errorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to delete item: {ex.Message}";
        }
    }

    // MODAL OPENING AND CLOSING FUNCTIONS
    private void OpenAddModal()
    {
        var uncategorized = _categories.FirstOrDefault(c => c.Name == "Uncategorized");
        var defaultCategoryId = uncategorized?.Id ?? 1;

        _viewModel = new InventoryItemViewModel
        {
            CategoryId = defaultCategoryId
        };

        _isAddModalVisible = true;
    }

    private void CloseAddModal()
    {
        _isAddModalVisible = false;
    }

    private void OpenFilterModal()
    {
        _isFilterModalVisible = true;
    }

    private void CloseFilterModal()
    {
        _isFilterModalVisible = false;
    }

    private void OpenItemDetailsModal(InventoryItem item)
    {
        _selectedItem = item;
        _isItemDetailsModalVisible = true;
    }

    private void CloseItemDetailsModal()
    {
        _isItemDetailsModalVisible = false;
        _selectedItem = null;
    }

    private void OpenEditItemModal(InventoryItem item)
    {
        _selectedItem = item;

        _viewModel = new InventoryItemViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Quantity = item.TotalQuantity,
            CategoryId = item.CategoryId,
            AvailabilityStatus = item.AvailabilityStatus,
            Weight = item.Weight,
            Height = item.Height,
            Width = item.Width,
            Length = item.Length,
            TruckLoadingPriority = item.TruckLoadingPriority,
            RentalStatus = item.RentalStatus,
            RentalPricePerDay = item.RentalPricePerDay,
            Condition = item.Condition,
            ConditionDescription = item.ConditionDescription
        };

        _isItemEditModalVisible = true;
    }

    private void CloseEditItemModal()
    {
        _isItemEditModalVisible = false;
        _selectedItem = null;
    }

    private void ApplyFilters()
    {
        CloseFilterModal();
    }

    private void ClearFilters()
    {
        _filters = new InventoryFilterViewModel();
        CloseFilterModal();
    }

    /// <summary>
    /// Returns the appropriate CSS class for condition badges based on item condition.
    /// </summary>
    /// <param name="condition">The items condition</param>
    /// <returns>CSS class</returns>
    private string GetConditionBadgeClass(Condition? condition)
    {
        if (!condition.HasValue) return "badge bg-secondary";

        return condition.Value switch
        {
            Condition.New => "badge bg-success",
            Condition.Damaged => "badge bg-warning text-dark",
            Condition.Lost => "badge bg-danger",
            _ => "badge bg-secondary"
        };
    }

    /// <summary>
    /// Returns the appropriate CSS class for availability status badges
    /// </summary>
    /// <param name="status">The items availability status</param>
    /// <returns>CSS class</returns>
    private string GetStatusBadgeClass(AvailabilityStatus status)
    {
        return status switch
        {
            AvailabilityStatus.Available => "badge bg-success",
            AvailabilityStatus.Unavailable => "badge bg-danger",
            _ => "badge bg-secondary"
        };
    }
}