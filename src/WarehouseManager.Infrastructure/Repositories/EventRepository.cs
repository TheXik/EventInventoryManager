using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;

/// <summary>
/// Handles all database operations for Event entities
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets all events from the database
    /// </summary>
    /// <returns>A list of all Event objects</returns>
    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.EventInventoryItems) // Načíta aj priradené položky
            .ToListAsync();
    }
    /// <summary>
    /// Gets an event by its ID
    /// </summary>
    /// <param name="id"> ID of the event to get </param>
    /// <returns>Event object or else null</returns>
    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.EventInventoryItems)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    /// <summary>
    /// Adds a new event to the database
    /// </summary>
    /// <param name="newEvent"> Name of the event to add</param>
    /// <returns>Return the newly added event</returns>
    public async Task<Event> AddAsync(Event newEvent)
    {
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        return newEvent;
    }
    /// <summary>
    /// Updates an existing event in the database
    /// </summary>
    /// <param name="updatedEvent"> Event to update </param>
    public async Task UpdateAsync(Event updatedEvent)
    {
        _context.Entry(updatedEvent).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// deletes an event from the database by its ID
    /// </summary>
    /// <param name="id"> Id of the event to delete </param>
    public async Task DeleteAsync(int id)
    {
        var eventToDelete = await _context.Events.FindAsync(id);
        if (eventToDelete != null)
        {
            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();
        }
    }
}