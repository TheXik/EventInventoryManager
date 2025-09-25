using WarehouseManager.Core.Entities.Rentals;

namespace WarehouseManager.Application.Interfaces;

/// <summary>
/// Provides access to individual rental line items that belong to a rental order
/// </summary>
public interface IRentalItemRepository
{
    /// <summary>
    /// Returns all rental items associated with a rental order
    /// </summary>
    /// <param name="rentalId">The rental identifier</param>
    /// <returns>An enumerable of rental items</returns>
    Task<IEnumerable<RentalItem>> GetByRentalIdAsync(int rentalId);

    /// <summary>
    /// Retrieves a single rental item by its identifier
    /// </summary>
    /// <param name="rentalItemId">The rental item identifier</param>
    /// <returns>The rental item if found, else null</returns>
    Task<RentalItem?> GetByIdAsync(int rentalItemId);

    /// <summary>
    /// Creates a new rental item line under a rental order.
    /// </summary>
    /// <param name="rentalItem">The rental item to create</param>
    Task AddAsync(RentalItem rentalItem);

    /// <summary>
    /// Updates an existing rental item line.
    /// </summary>
    /// <param name="rentalItem">The rental item with updated fields</param>
    Task UpdateAsync(RentalItem rentalItem);

    /// <summary>
    /// Deletes a rental item line by its identifier.
    /// </summary>
    /// <param name="rentalItemId">The rental item identifier</param>
    Task DeleteAsync(int rentalItemId);
}