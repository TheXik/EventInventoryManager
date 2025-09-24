namespace WarehouseManager.Core.Entities;

/// <summary>
/// Represents a scheduled event for which user can also add inventory items that are
/// suposed to be used at the event
/// </summary>
public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ClientName { get; set; }

    /// <summary>
    /// Contact information for the client
    /// </summary>
    public string? ClientContact { get; set; }

    /// <summary>
    /// Location of the event
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Additional description or notes about the event.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional color tag for the event used for UI display
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Collection of inventory items allocated to this event
    /// </summary>
    public ICollection<EventInventoryItem> EventInventoryItems { get; set; }
}