using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;

public class InventoryItemRepository : IInventoryItemRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByIdAsync(int id)
    {
        return await _context.InventoryItems.FindAsync(id);
    }

    public async Task<IEnumerable<InventoryItem>> GetAllAsync()
    {
        return await _context.InventoryItems.ToListAsync();
    }

    public async Task AddAsync(InventoryItem item)
    {
        await _context.InventoryItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(InventoryItem item)
    {
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}