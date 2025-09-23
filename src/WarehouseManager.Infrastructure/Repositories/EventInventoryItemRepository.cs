using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;

public class EventInventoryItemRepository : IEventInventoryItemRepository
{
    private readonly ApplicationDbContext _context;

    public EventInventoryItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventInventoryItem>> GetByEventIdAsync(int eventId)
    {
        // the database ignoring any in-memory change when asNoTracking is used
        return await _context.EventInventoryItems
            .AsNoTracking()
            .Where(ei => ei.EventId == eventId)
            .ToListAsync();
    }


    public async Task AddAsync(EventInventoryItem entity)
    {
        await _context.EventInventoryItems.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(EventInventoryItem entity)
    {
        _context.EventInventoryItems.Update(entity);
        await _context.SaveChangesAsync();
    }

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