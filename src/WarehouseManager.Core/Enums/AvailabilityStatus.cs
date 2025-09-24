namespace WarehouseManager.Core.Enums;

/// <summary>
/// This Enu helps identify which inventory item is currently available for use
/// </summary>
public enum AvailabilityStatus
{
    /// <summary>
    /// The item has at least one unit available
    /// </summary>
    Available,
    /// <summary>
    /// No units are currently available
    /// </summary>
    Unavailable
}