using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Application.Interfaces;

/// <summary>
/// Interface for reading and writing item categories used to group inventory items
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Returns all defined item categories
    /// </summary>
    /// <returns>A list of categories. Returns an empty list if NO items exist.</returns>
    Task<List<ItemCategory>> GetAllAsync();

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="category">The category to create.</param>
    /// <returns>The created category </returns>
    Task<ItemCategory> AddAsync(ItemCategory category);
}