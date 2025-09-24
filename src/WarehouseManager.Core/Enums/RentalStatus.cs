namespace WarehouseManager.Core.Enums;

/// <summary>
/// Indicates whether an inventory item is currently used in rental 
/// </summary>
public enum RentalStatus
{
    /// <summary>
    /// The item is part of the rental catalog and can be rented
    /// </summary>
    Rented,
    /// <summary>
    /// The item is not used for rentals
    /// </summary>
    NotInRentalUse
}