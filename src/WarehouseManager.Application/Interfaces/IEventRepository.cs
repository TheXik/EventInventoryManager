using WarehouseManager.Core.Entities;

namespace WarehouseManager.Application.Interfaces;

/// <summary>
/// Managing events within the warehouse 
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Retrieves all events
    /// </summary>
    /// <returns>An enumerable of events. Empty if no events exist</returns>
    Task<IEnumerable<Event>> GetAllAsync();

    /// <summary>
    /// Retrieves a single event by its identifier
    /// </summary>
    /// <param name="id">The event identifier</param>
    /// <returns>The event if found, else null</returns>
    Task<Event?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new event
    /// </summary>
    /// <param name="newEvent">The event to create</param>
    /// <returns>The created event with any generated values</returns>
    Task<Event> AddAsync(Event newEvent);

    /// <summary>
    /// Updates an existing event
    /// </summary>
    /// <param name="updatedEvent">The event with updated fields</param>
    Task UpdateAsync(Event updatedEvent);

    /// <summary>
    /// Deletes an event by its identifier
    /// </summary>
    /// <param name="id">The event identifier</param>
    Task DeleteAsync(int id);
}