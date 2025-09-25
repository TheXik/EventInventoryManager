using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;


/// <summary>
/// Handles all database operations for EventInventoryItem entities
/// </summary>
public class EventInventoryItemRepository : IEventInventoryItemRepository
{
    
    private readonly ApplicationDbContext _context;
    
    /// <summary>
    /// Injects the database context
    /// </summary>
    /// <param name="context"> The database context from dependency injection </param>
    public EventInventoryItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets all EventInventoryItems for a specific event
    /// </summary>
    /// <param name="eventId"> ID of the event </param>
    /// <returns> List of EventInventoryItem objects </returns>
    public async Task<IEnumerable<EventInventoryItem>> GetByEventIdAsync(int eventId)
    {
        return await _context.EventInventoryItems
            .AsNoTracking()
            .Where(ei => ei.EventId == eventId)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new EventInventoryItem to the database
    /// </summary>
    /// <param name="entity"> EventInventoryItem to add </param>
    public async Task AddAsync(EventInventoryItem entity)
    {
        await _context.EventInventoryItems.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Updates an existing EventInventoryItem in the database
    /// </summary>
    /// <param name="entity"> EventInventoryItem to update </param>
    public async Task UpdateAsync(EventInventoryItem entity)
    {
        _context.EventInventoryItems.Update(entity);
        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// Deletes an EventInventoryItem from the database
    /// </summary>
    /// <param name="eventId">Event ID of the EventInventoryItem to delete</param>
    /// <param name="inventoryItemId"> inventory item ID of the EventInventoryItem to delete</param>
    public async Task DeleteAsync(int eventId, int inventoryItemId)
    {
        var entity = await _context.EventInventoryItems
            .FindAsync(eventId, inventoryItemId);
        
        if (entity != null)
        {
            _context.EventInventoryItems.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}