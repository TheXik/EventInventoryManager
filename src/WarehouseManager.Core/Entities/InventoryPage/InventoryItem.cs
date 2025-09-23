using WarehouseManager.Core.Enums;

namespace WarehouseManager.Core.Entities.InventoryPage;

public class InventoryItem
{
    // Basic Information
    public int Id { get; set; }
    public required string Name { get; set; }

    public string? Description { get; set; }
    
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }

    public int CategoryId { get; set; }
    public required ItemCategory Category { get; set; }

    public AvailabilityStatus AvailabilityStatus { get; set; }
    
    public ICollection<EventInventoryItem> EventInventoryItems { get; set; }

    // Dimensions
    public int Weight { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public TruckLoadingPriority? TruckLoadingPriority { get; set; } // for optimization of truck loading 
    
    //TODO LATER 

    // // Image
    // public string? ImageUrl { get; set; }
    //
    // Rental Information
    public RentalStatus RentalStatus { get; set; }
    public DateTime RentalDate { get; set; }
    public decimal RentalPricePerDay { get; set; }

    public string? RentalDescription { get; set; }

    //
    // Condition
    public Condition? Condition { get; set; }
    public string? ConditionDescription { get; set; }
    
    
    public void UpdateAvailabilityStatus()
    {
        if (AvailableQuantity > 0)
        {
            AvailabilityStatus = AvailabilityStatus.Available;
        }
        else
        {
            AvailabilityStatus = AvailabilityStatus.Unavailable;
        }
    }
    
}