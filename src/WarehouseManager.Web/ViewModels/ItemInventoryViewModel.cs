using System.ComponentModel.DataAnnotations;
using WarehouseManager.Core.Enums;

namespace EventInventoryManager.ViewModels;

/// <summary>
///     This class is for the InventoryItemViewModel for the Inventory page Add new item form.
/// </summary>
public class InventoryItemViewModel
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


    // This property is only used if the user wants to create a new category
    public string? NewCategoryName { get; set; }


    // --- Handling Enums ---
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