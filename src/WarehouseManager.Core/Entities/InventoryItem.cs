using WarehouseManager.Core.Enums;

namespace WarehouseManager.Core.Entities;

public class InventoryItem
{
    // Basic Information
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int OnStockQuantity { get; set; } // TODO later maybe delete right now im not sure if i would need this
    public int Quantity { get; set; }
    public ItemCategory? Category { get; set; }
    public AvailabilityStatus AvailabilityStatus { get; set; }
    
   
    // Dimensions
    public int Weight { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    //public TruckLoadingPriority? TruckLoadingPriority { get; set; } // for optimization of truck loading //TODO
    //TODO LATER 
    
    // // Image
    // public string? ImageUrl { get; set; }
    //
    // // Rental Information
    // public RentalStatus RentalStatus { get; set; }
    // public DateTime RentalDate { get; set; }
    // public int RentalPrice { get; set; }
    // public string? RentalDescription { get; set; }
    //
    // // Condition
    // public Condition? Condition { get; set; }
    // public string? ConditionDescription { get; set; }
    //
    
    
}

