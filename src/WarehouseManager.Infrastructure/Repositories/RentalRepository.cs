using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;
/// <summary>
/// Handles all database operations for Rental entities
/// </summary>
public class RentalRepository : IRentalRepository
{
    private readonly ApplicationDbContext _context;

    public RentalRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Gets all rentals from the database
    /// </summary>
    /// <returns> List of all rentals </returns>
    public async Task<IEnumerable<Rental>> GetAllAsync()
    {
        return await _context.Rentals
            .Include(r => r.RentalItems)
            .ThenInclude(ri => ri.InventoryItem)
            .ToListAsync();
    }
    /// <summary>
    /// Gets a rental by its ID
    /// </summary>
    /// <param name="id">Id of the rental</param>
    /// <returns>Rental with the given ID or null if not found</returns>
    public async Task<Rental?> GetByIdAsync(int id)
    {
        return await _context.Rentals
            .Include(r => r.RentalItems)
            .ThenInclude(ri => ri.InventoryItem)
            .FirstOrDefaultAsync(r => r.RentalId == id);
    }
    /// <summary>
    /// Adds a new rental to the database
    /// </summary>
    /// <param name="rental">rental to add</param>
    /// <returns>Rental that was added</returns>
    public async Task<Rental> AddAsync(Rental rental)
    {
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();
        return rental;
    }
    /// <summary>
    /// Updates an existing rental in the database
    /// </summary>
    /// <param name="rental"> Rental to update </param>
    public async Task UpdateAsync(Rental rental)
    {
        // Update only scalar properties to avoid unintended changes to navigation collections (RentalItems, InventoryItem)
        _context.Rentals.Attach(rental);
        var entry = _context.Entry(rental);

        entry.Property(r => r.ClientName).IsModified = true;
        entry.Property(r => r.ContactInfo).IsModified = true;
        entry.Property(r => r.RentalDate).IsModified = true;
        entry.Property(r => r.ExpectedReturnDate).IsModified = true;
        entry.Property(r => r.ActualReturnDate).IsModified = true;
        entry.Property(r => r.Status).IsModified = true;
        entry.Property(r => r.PaymentStatus).IsModified = true;
        entry.Property(r => r.DiscountPercentage).IsModified = true;
        entry.Property(r => r.Notes).IsModified = true;

        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// Deletes a rental from the database but also returns the stock of the rental items to the inventory
    /// </summary>
    /// <param name="id">Id of the rental to delete</param>
    public async Task DeleteAsync(int id)
    {
        // Load rental with items and their inventory items to restore stock
        var rental = await _context.Rentals
            .Include(r => r.RentalItems)
            .ThenInclude(ri => ri.InventoryItem)
            .FirstOrDefaultAsync(r => r.RentalId == id);

        if (rental == null) return;

        // Return stock: add back unreturned quantities
        foreach (var ri in rental.RentalItems)
        {
            var toReturn = Math.Max(0, ri.QuantityRented - ri.QuantityReturned);
            var inv = ri.InventoryItem ??
                      await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == ri.InventoryItemId);
            if (inv != null)
            {
                if (toReturn > 0)
                {
                    inv.AvailableQuantity += toReturn;
                    inv.UpdateAvailabilityStatus();
                }

                inv.RentalStatus = RentalStatus.NotInRentalUse;
            }
        }

        await _context.SaveChangesAsync();

        // Now remove the rental
        _context.Rentals.Remove(rental);
        await _context.SaveChangesAsync();
    }
}