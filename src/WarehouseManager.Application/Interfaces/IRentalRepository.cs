using WarehouseManager.Core.Entities.Rentals;

namespace WarehouseManager.Application.Interfaces;

public interface IRentalRepository
{
    Task<IEnumerable<Rental>> GetAllAsync();
    Task<Rental?> GetByIdAsync(int id);
    Task<Rental> AddAsync(Rental rental);
    Task UpdateAsync(Rental rental);
    Task DeleteAsync(int id);
}