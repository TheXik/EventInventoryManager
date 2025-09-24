using System.ComponentModel.DataAnnotations;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Core.Entities.Rentals;

/// <summary>
/// Represents a rental order made by a client, including items and key dates
/// </summary>
public class Rental
{
    [Key] 
    public int RentalId { get; set; }
    
    [Required] 
    public string ClientName { get; set; }

    /// <summary>
    /// Contact information for the client phone /email
    /// </summary>
    public string ContactInfo { get; set; } = string.Empty;

    /// <summary>
    /// Date when the rental begins
    /// </summary>
    public DateTime RentalDate { get; set; }

    /// <summary>
    /// Expected date when all items should be returned
    /// </summary>
    public DateTime ExpectedReturnDate { get; set; }

    /// <summary>
    /// Actual date when all items were returned, if completed
    /// </summary>
    public DateTime? ActualReturnDate { get; set; }

    /// <summary>
    /// Overall status of the rental order
    /// </summary>
    public RentalOrderStatus Status { get; set; } = RentalOrderStatus.Draft;

    /// <summary>
    /// Payment state for this rental
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    /// <summary>
    /// Percentage discount applied to the order total
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Additional notes or instructions associated with the rental
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Collection of line items included in the rental order
    /// </summary>
    public ICollection<RentalItem> RentalItems { get; set; } = new List<RentalItem>();
}