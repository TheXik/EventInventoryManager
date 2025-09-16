using WarehouseManager.Core.Entities;

namespace WarehouseManager.Application.Interfaces;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
    Task<Event> AddAsync(Event newEvent);
    Task UpdateAsync(Event updatedEvent);
    Task DeleteAsync(int id);
}