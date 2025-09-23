using System.ComponentModel.DataAnnotations;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Core.Entities.Rentals;

public class Rental
{
    [Key]
    public int RentalId { get; set; }

    [Required]
    public string ClientName { get; set; }

    public string ContactInfo { get; set; } = string.Empty;

    public DateTime RentalDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    public RentalOrderStatus Status { get; set; } = RentalOrderStatus.Draft;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;


    public decimal DiscountPercentage { get; set; }

    public string? Notes { get; set; }

    public ICollection<RentalItem> RentalItems { get; set; } = new List<RentalItem>();
}