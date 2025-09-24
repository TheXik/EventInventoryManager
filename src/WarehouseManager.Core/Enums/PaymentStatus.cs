namespace WarehouseManager.Core.Enums;

/// <summary>
/// Represents the payment state of a rental order
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// No payment has been received yet
    /// </summary>
    Unpaid,
    /// <summary>
    /// Payment has been fully received
    /// </summary>
    Paid,
    /// <summary>
    /// Payment will be handled via invoice
    /// </summary>
    Invoice
}