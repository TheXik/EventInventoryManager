using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Enums;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;
/// <summary>
/// Handles all database operations for InventoryItem entities
/// </summary>
public class InventoryItemRepository : IInventoryItemRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets an InventoryItem by its ID
    /// </summary>
    /// <param name="id"> Id of the InventoryItem to get </param>
    /// <returns>Return the InventoryItem with the given ID or null if not found</returns>
    public async Task<InventoryItem?> GetByIdAsync(int id)
    {
        return await _context.InventoryItems.FindAsync(id);
    }
    
    /// <summary>
    /// Gets all the InventoryItems from the database
    /// </summary>
    /// <returns>List of all InventoryItem objects</returns>
    public async Task<IEnumerable<InventoryItem>> GetAllAsync()
    {
        return await _context.InventoryItems.ToListAsync();
    }
    
    
    /// <summary>
    /// Adds a new InventoryItem to the database
    /// </summary>
    /// <param name="item"> Name of the InventoryItem to add </param>
    public async Task AddAsync(InventoryItem item)
    {
        await _context.InventoryItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// Updates an existing InventoryItem in the database
    /// </summary>
    /// <param name="item"> Name of the InventoryItem to update </param>
    public async Task UpdateAsync(InventoryItem item)
    {
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// Deletes an InventoryItem from the database
    /// </summary>
    /// <param name="id"> Name of the InventoryItem to delete </param>
    /// <exception cref="InvalidOperationException"> Thrown if the InventoryItem is referenced in one or more rentals </exception>
    public async Task DeleteAsync(int id)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
            try
            {
                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "Cannot delete this item because it is referenced in one or more rentals. Remove the item from those rentals or mark rentals as returned before deleting.",
                    ex);
            }
    }
    /// <summary>
    /// Counts the number of active rentals for a given InventoryItem
    /// </summary>
    /// <param name="inventoryItemId"></param>
    /// <returns></returns>
    public async Task<int> CountActiveRentalReferencesAsync(int inventoryItemId)
    {
        return await _context.RentalItems
            .Include(ri => ri.Rental)
            .CountAsync(ri => ri.InventoryItemId == inventoryItemId && ri.Rental.Status != RentalOrderStatus.Returned);
    }
}