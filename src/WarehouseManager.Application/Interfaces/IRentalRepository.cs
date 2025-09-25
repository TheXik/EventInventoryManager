using WarehouseManager.Core.Entities.Rentals;

namespace WarehouseManager.Application.Interfaces;

/// <summary>
/// Interface for managing rental orders, including their lifecycle and status.
/// </summary>
public interface IRentalRepository
{
    /// <summary>
    /// Retrieves all rental orders
    /// </summary>
    /// <returns>An enumerable of rentals</returns>
    Task<IEnumerable<Rental>> GetAllAsync();

    /// <summary>
    /// Retrieves a rental order by its ID
    /// </summary>
    /// <param name="id">The rental ID</param>
    /// <returns>The rental if found, else null</returns>
    Task<Rental?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new rental order
    /// </summary>
    /// <param name="rental">The rental to create</param>
    /// <returns>The created rental</returns>
    Task<Rental> AddAsync(Rental rental);

    /// <summary>
    /// Updates an existing rental order
    /// </summary>
    /// <param name="rental">The rental with updated fields</param>
    Task UpdateAsync(Rental rental);

    /// <summary>
    /// Deletes a rental order by its ID
    /// </summary>
    /// <param name="id">The rental ID</param>
    Task DeleteAsync(int id);
}