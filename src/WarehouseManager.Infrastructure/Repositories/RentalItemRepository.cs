using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;

/// <summary>
/// Handles all database operations for RentalItem entities
/// </summary>
public class RentalItemRepository : IRentalItemRepository
{
    private readonly ApplicationDbContext _context;

    public RentalItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Gets all RentalItems for a given Rental
    /// </summary>
    /// <param name="rentalId">Id of the Rental</param>
    /// <returns>List of RentalItems</returns>
    public async Task<IEnumerable<RentalItem>> GetByRentalIdAsync(int rentalId)
    {
        return await _context.RentalItems
            .Include(ri => ri.InventoryItem)
            .Where(ri => ri.RentalId == rentalId)
            .ToListAsync();
    }
    /// <summary>
    /// Gets a RentalItem by its ID
    /// </summary>
    /// <param name="rentalItemId" ID of the RentalItem></param>
    /// <returns>RentalItem with the given ID or null if not found</returns>
    public async Task<RentalItem?> GetByIdAsync(int rentalItemId)
    {
        return await _context.RentalItems
            .Include(ri => ri.InventoryItem)
            .FirstOrDefaultAsync(ri => ri.RentalItemId == rentalItemId);
    }
    /// <summary>
    /// Adds a new RentalItem to the database
    /// </summary>
    /// <param name="rentalItem"> Name of the RentalItem to add</param>
    public async Task AddAsync(RentalItem rentalItem)
    {
        await _context.RentalItems.AddAsync(rentalItem);
        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// Updates an existing RentalItem in the database
    /// </summary>
    /// <param name="rentalItem">Name of the item to update</param>
    public async Task UpdateAsync(RentalItem rentalItem)
    {
        // Update only scalar properties; avoid changing navigation or foreign keys inadvertently
        _context.RentalItems.Attach(rentalItem);
        var entry = _context.Entry(rentalItem);
        entry.Property(ri => ri.QuantityRented).IsModified = true;
        entry.Property(ri => ri.QuantityReturned).IsModified = true;
        entry.Property(ri => ri.PricePerDayAtTimeOfRental).IsModified = true;
        // Do not modify RentalId/InventoryItemId here to avoid relationship problems
        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// Deletes a RentalItem from the database
    /// </summary>
    /// <param name="rentalItemId"> ID of the RentalItem to delete</param>
    public async Task DeleteAsync(int rentalItemId)
    {
        var entity = await _context.RentalItems.FindAsync(rentalItemId);
        if (entity != null)
        {
            _context.RentalItems.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}