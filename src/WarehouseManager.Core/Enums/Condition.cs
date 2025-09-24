namespace WarehouseManager.Core.Enums;

/// <summary>
/// Describes the physical state of an inventory item
/// </summary>
public enum Condition
{
    /// <summary>
    /// The item is new or like new
    /// </summary>
    New,
    /// <summary>
    /// The item has damage that may affect use or appearance
    /// </summary>
    Damaged,
    /// <summary>
    /// The item is missing and cannot be used
    /// </summary>
    Lost
}