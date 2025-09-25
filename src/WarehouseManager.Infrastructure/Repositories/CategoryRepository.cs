using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;

/// <summary>
/// Handles all database operations for ItemCategory entities
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the CategoryRepository
    /// </summary>
    /// <param name="context">The database context from dependency injection</param>
    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a list of all item categories from the database
    /// </summary>
    /// <returns>A list of all ItemCategory objects</returns>
    public async Task<List<ItemCategory>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    /// <summary>
    /// Adds a new category to the database.
    /// </summary>
    /// <param name="category">The new category to add</param>
    /// <returns>The category that was added</returns>
    public async Task<ItemCategory> AddAsync(ItemCategory category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }
}