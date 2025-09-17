using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // CORRECTED METHOD: This now fetches ItemCategory objects
    public async Task<List<ItemCategory>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<ItemCategory> AddAsync(ItemCategory category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }
}