using WarehouseManager.Core.Entities;

namespace WarehouseManager.Application.Interfaces
{
    public interface IEventInventoryItemRepository
    {
        Task<IEnumerable<EventInventoryItem>> GetByEventIdAsync(int eventId);
        Task AddAsync(EventInventoryItem entity);
        Task UpdateAsync(EventInventoryItem entity);
        Task DeleteAsync(int eventId, int inventoryItemId);
    }
}