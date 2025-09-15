using WarehouseManager.Core.Entities;

namespace WarehouseManager.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<ItemCategory>> GetAllAsync();
    Task<ItemCategory> AddAsync(ItemCategory category);
}