using WarehouseManager.Core.Entities;

namespace WarehouseManager.Application.Interfaces;

/// <summary>
/// Provides access to the items assigned to a specific event. Loading items for an event
/// </summary>
public interface IEventInventoryItemRepository
{
    /// <summary>
    /// Returns all event inventory links for a given event
    /// </summary>
    /// <param name="eventId">The event identifier</param>
    /// <returns>An enumerable of EventInventoryItem entries for the event</returns>
    Task<IEnumerable<EventInventoryItem>> GetByEventIdAsync(int eventId);

    /// <summary>
    /// Adds a new item assignment for an event
    /// </summary>
    /// <param name="entity">The event-inventory record to create</param>
    Task AddAsync(EventInventoryItem entity);

    /// <summary>
    /// Updates quantity or other details of an item assigned to an event
    /// </summary>
    /// <param name="entity">The updated event-inventory record</param>
    Task UpdateAsync(EventInventoryItem entity);

    /// <summary>
    /// Removes an item from an event
    /// </summary>
    /// <param name="eventId">The event identifier</param>
    /// <param name="inventoryItemId">The inventory item identifier</param>
    Task DeleteAsync(int eventId, int inventoryItemId);
}