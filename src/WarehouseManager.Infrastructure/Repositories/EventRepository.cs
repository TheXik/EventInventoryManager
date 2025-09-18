using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.EventInventoryItems) // Načíta aj priradené položky
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.EventInventoryItems)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event> AddAsync(Event newEvent)
    {
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        return newEvent;
    }

    public async Task UpdateAsync(Event updatedEvent)
    {
        _context.Entry(updatedEvent).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

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