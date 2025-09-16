using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<ItemCategory>> GetAllAsync();
    Task<ItemCategory> AddAsync(ItemCategory category);
}