using WarehouseManager.Core.Entities;

namespace WarehouseManager.Application.Interfaces;

public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<IEnumerable<InventoryItem>> GetAllAsync();
    Task AddAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
    Task DeleteAsync(int id);
}