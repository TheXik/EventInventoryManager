using System.ComponentModel.DataAnnotations;
using Microsoft.JSInterop;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages;

//  view model keep within the UI layer
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

    public TruckLoadingPriority? TruckLoadingPriority { get; set; }
    public RentalStatus RentalStatus { get; set; } = RentalStatus.NotInRentalUse;
    public DateTime? RentalDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
    public decimal RentalPricePerDay { get; set; }

    public string? RentalDescription { get; set; }
    public Condition? Condition { get; set; }
    public string? ConditionDescription { get; set; }
}

internal class InventoryFilterViewModel
{
    public HashSet<int> SelectedCategoryIds { get; set; } = new();
    public HashSet<AvailabilityStatus> SelectedAvailabilityStatuses { get; set; } = new();
    public HashSet<Condition> SelectedConditions { get; set; } = new();
}

public partial class Inventory
{
    private List<InventoryItem>? _allItems;
    private List<ItemCategory> _categories = new(); // For the dropdown
    private string? _errorMessage;

    private InventoryFilterViewModel _filters = new();
    private bool _isAddModalVisible;
    private bool _isFilterModalVisible;
    private bool _isItemDetailsModalVisible;
    private bool _isItemEditModalVisible;
    private string _searchQuery = "";
    private InventoryItem? _selectedItem; // To store the item being viewed
    private InventoryItemViewModel _viewModel = new();

    private IEnumerable<InventoryItem> FilteredItems
    {
        get
        {
            if (_allItems is null) return Enumerable.Empty<InventoryItem>();

            var query = _allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_searchQuery))
                query = query.Where(item =>
                    item.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (item.Description != null &&
                     item.Description.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)));

            if (_filters.SelectedCategoryIds.Any())
                query = query.Where(item => _filters.SelectedCategoryIds.Contains(item.CategoryId));

            if (_filters.SelectedAvailabilityStatuses.Any())
                query = query.Where(item => _filters.SelectedAvailabilityStatuses.Contains(item.AvailabilityStatus));

            if (_filters.SelectedConditions.Any())
                query = query.Where(item =>
                    item.Condition.HasValue && _filters.SelectedConditions.Contains(item.Condition.Value));

            return query.ToList();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        // Get all data from the database
        var itemsTask = InventoryRepo.GetAllAsync();
        var categoriesTask = CategoryRepo.GetAllAsync();
        await Task.WhenAll(itemsTask, categoriesTask);

        _allItems = (await itemsTask).ToList();
        _categories = (await categoriesTask).ToList();
    }

    private void ToggleSelection<T>(HashSet<T> set, T value)
    {
        if (set.Contains(value))
            set.Remove(value);
        else
            set.Add(value);
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            if (_viewModel.CategoryId == -1)
            {
                if (string.IsNullOrWhiteSpace(_viewModel.NewCategoryName))
                {
                    _errorMessage = "Please provide a name for the new category.";
                    return;
                }

                var newCategory = new ItemCategory { Name = _viewModel.NewCategoryName };
                var createdCategory = await CategoryRepo.AddAsync(newCategory);
                _viewModel.CategoryId = createdCategory.Id;
            }

            var newItem = new InventoryItem
            {
                Name = _viewModel.Name,
                Description = _viewModel.Description,
                AvailableQuantity = _viewModel.Quantity,
                TotalQuantity = _viewModel.Quantity,
                CategoryId = _viewModel.CategoryId,
                AvailabilityStatus = _viewModel.AvailabilityStatus,
                Weight = _viewModel.Weight,
                Height = _viewModel.Height,
                Width = _viewModel.Width,
                TruckLoadingPriority = _viewModel.TruckLoadingPriority,
                RentalStatus = _viewModel.RentalStatus,
                RentalDate = _viewModel.RentalDate ?? DateTime.UtcNow,
                RentalPricePerDay = _viewModel.RentalPricePerDay,
                Condition = _viewModel.Condition,
                ConditionDescription = _viewModel.ConditionDescription,
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

    private async Task HandleUpdateSubmit()
    {
        try
        {
            var itemToUpdate = _allItems?.FirstOrDefault(i => i.Id == _viewModel.Id);
            if (itemToUpdate is null)
            {
                _errorMessage = "The item you are trying to update was not found.";
                return;
            }

            itemToUpdate.Name = _viewModel.Name;
            itemToUpdate.Description = _viewModel.Description;
            itemToUpdate.TotalQuantity = _viewModel.Quantity;
            itemToUpdate.CategoryId = _viewModel.CategoryId;
            itemToUpdate.AvailabilityStatus = _viewModel.AvailabilityStatus;
            itemToUpdate.Weight = _viewModel.Weight;
            itemToUpdate.Height = _viewModel.Height;
            itemToUpdate.Width = _viewModel.Width;
            itemToUpdate.TruckLoadingPriority = _viewModel.TruckLoadingPriority;
            itemToUpdate.RentalStatus = _viewModel.RentalStatus;
            itemToUpdate.RentalPricePerDay = _viewModel.RentalPricePerDay;
            itemToUpdate.Condition = _viewModel.Condition;
            itemToUpdate.ConditionDescription = _viewModel.ConditionDescription;

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

    private async Task DeleteItem(InventoryItem item)
    {
        try
        {
            // Pre-check active rental references
            var activeRefs = await InventoryRepo.CountActiveRentalReferencesAsync(item.Id);
            if (activeRefs > 0)
            {
                var confirm = await JS.InvokeAsync<bool>("confirm",
                    $"This item is included in {activeRefs} active rental(s). If you delete it, those rental records may be affected. Are you sure you want to proceed?");
                if (!confirm) return;
            }

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