using WarehouseManager.Core.Enums;

namespace WarehouseManager.Core.Entities.InventoryPage;

/// <summary>
/// Represents a physical inventory item that can be allocated to events or rented
/// </summary>
public class InventoryItem
{
    // Basic Information
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the item
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Total units of this item available in stock
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Units currently available (not allocated to event or rented)
    /// </summary>
    public int AvailableQuantity { get; set; }

    /// <summary>
    /// Foreign key to the item category
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Navigation to the category this item belongs to
    /// </summary>
    public required ItemCategory Category { get; set; }

    /// <summary>
    /// Computed availability based on AvailableQuantity
    /// </summary>
    public AvailabilityStatus AvailabilityStatus { get; set; }

    /// <summary>
    /// Links to event allocations containing this item
    /// </summary>
    public ICollection<EventInventoryItem> EventInventoryItems { get; set; }

    // Dimensions
    /// <summary>
    /// Weight of a single unit in kilograms 
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// Height dimension of a single unit
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Width dimension of a single unit
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Length dimension of a single unit
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Loading priority suggestion to optimize truck loading
    /// </summary>
    public TruckLoadingPriority? TruckLoadingPriority { get; set; }

    //TODO LATER 
    // Image
    // public string? ImageUrl { get; set; }
    /// <summary>
    /// Indicates whether at least one item is being rented
    /// </summary>
    public RentalStatus RentalStatus { get; set; }

    /// <summary>
    /// The date when this item was added to rental or last rented
    /// </summary>
    public DateTime RentalDate { get; set; }

    /// <summary>
    /// Price per day when rented
    /// </summary>
    public decimal RentalPricePerDay { get; set; }

    /// <summary>
    /// Notes related to the rental use of the item
    /// </summary>
    public string? RentalDescription { get; set; }

    //
    // Condition
    /// <summary>
    /// Current physical condition of the item
    /// </summary>
    public Condition? Condition { get; set; }

    /// <summary>
    /// Optional details about the condition.
    /// </summary>
    public string? ConditionDescription { get; set; }

    /// <summary>
    /// Updates AvailabilityStatus based on AvailableQuantity.
    /// </summary>
    public void UpdateAvailabilityStatus()
    {
        if (AvailableQuantity > 0)
            AvailabilityStatus = AvailabilityStatus.Available;
        else
            AvailabilityStatus = AvailabilityStatus.Unavailable;
    }
}