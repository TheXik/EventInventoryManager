using WarehouseManager.Core.Entities.Rentals;

namespace WarehouseManager.Application.Interfaces;

public interface IRentalItemRepository
{
    Task<IEnumerable<RentalItem>> GetByRentalIdAsync(int rentalId);
    Task<RentalItem?> GetByIdAsync(int rentalItemId);
    Task AddAsync(RentalItem rentalItem);
    Task UpdateAsync(RentalItem rentalItem);
    Task DeleteAsync(int rentalItemId);
}