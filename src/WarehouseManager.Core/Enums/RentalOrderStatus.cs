namespace WarehouseManager.Core.Enums;

/// <summary>
/// Represents the overall status of a rental order throughout its lifecycle
/// </summary>
public enum RentalOrderStatus
{
    /// <summary>
    /// The order is being prepared and is not finalized
    /// </summary>
    Draft,

    /// <summary>
    /// Items have been rented out to the client
    /// </summary>
    Rented,

    /// <summary>
    /// All rented items have been returned
    /// </summary>
    Returned,

    /// <summary>
    /// The return is past the expected date
    /// </summary>
    Overdue
}