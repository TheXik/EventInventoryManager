namespace WarehouseManager.Core.Enums;

/// <summary>
/// Priority level used to optimize the order in which items are loaded into a truck
/// </summary>
public enum TruckLoadingPriority
{
    /// <summary>
    /// Lowest priority for loading
    /// </summary>
    Lowest,
    
    /// <summary>
    /// Low priority for loading
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium priority for loading
    /// </summary>
    Medium,
    
    /// <summary>
    /// High priority for loading
    /// </summary>
    High,
    
    /// <summary>
    /// Highest priority for loading
    /// </summary>
    Highest
}