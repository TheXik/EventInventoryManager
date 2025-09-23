using Microsoft.EntityFrameworkCore;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Infrastructure.Data;

namespace WarehouseManager.Infrastructure.Repositories;

public class RentalItemRepository : IRentalItemRepository
{
    private readonly ApplicationDbContext _context;

    public RentalItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RentalItem>> GetByRentalIdAsync(int rentalId)
    {
        return await _context.RentalItems
            .Include(ri => ri.InventoryItem)
            .Where(ri => ri.RentalId == rentalId)
            .ToListAsync();
    }

    public async Task<RentalItem?> GetByIdAsync(int rentalItemId)
    {
        return await _context.RentalItems
            .Include(ri => ri.InventoryItem)
            .FirstOrDefaultAsync(ri => ri.RentalItemId == rentalItemId);
    }

    public async Task AddAsync(RentalItem rentalItem)
    {
        await _context.RentalItems.AddAsync(rentalItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RentalItem rentalItem)
    {
        // Update only scalar properties; avoid changing navigation or foreign keys inadvertently
        _context.RentalItems.Attach(rentalItem);
        var entry = _context.Entry(rentalItem);
        entry.Property(ri => ri.QuantityRented).IsModified = true;
        entry.Property(ri => ri.QuantityReturned).IsModified = true;
        entry.Property(ri => ri.PricePerDayAtTimeOfRental).IsModified = true;
        // Do not modify RentalId/InventoryItemId here to avoid relationship issues
        await _context.SaveChangesAsync();
    }

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