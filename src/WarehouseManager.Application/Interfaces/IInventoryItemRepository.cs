using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Application.Interfaces;

/// <summary>
/// Interface for Inventory items stored in the warehouse
/// </summary>
public interface IInventoryItemRepository
{
    /// <summary>
    /// Loads a single inventory item by its identifier
    /// </summary>
    /// <param name="id">The item identifier</param>
    /// <returns>The item if found, else null</returns>
    Task<InventoryItem?> GetByIdAsync(int id);

    /// <summary>
    /// Returns all inventory items.
    /// </summary>
    /// <returns>An enumerable of inventory items</returns>
    Task<IEnumerable<InventoryItem>> GetAllAsync();

    /// <summary>
    /// Creates a new inventory item
    /// </summary>
    /// <param name="item">The item to create</param>
    Task AddAsync(InventoryItem item);

    /// <summary>
    /// Any new changes to an existing inventory item
    /// </summary>
    /// <param name="item">The item with updated fields</param>
    Task UpdateAsync(InventoryItem item);

    /// <summary>
    /// Deletes an inventory item by its identifier
    /// </summary>
    /// <param name="id">The item identifier</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Counts how many active rentals still reference the specified inventory item
    /// This is used to enforce delete restrictions or to display warnings
    /// </summary>
    /// <param name="inventoryItemId">The inventory item identifier</param>
    /// <returns>The number of active rental references</returns>
    Task<int> CountActiveRentalReferencesAsync(int inventoryItemId);
}