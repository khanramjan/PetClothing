namespace PetClothingShop.Core.Entities;

/// <summary>
/// Stores payment transaction history for audit and reconciliation
/// </summary>
public class Payment
{
    public int Id { get; set; }
    public int? OrderId { get; set; }
    public int UserId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty; // Stripe Payment Intent ID
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Status { get; set; } = string.Empty; // requires_payment_method, processing, succeeded, failed, canceled, requires_action
    public string PaymentMethod { get; set; } = "stripe"; // stripe, paypal, etc.
    public string? PaymentMethodDetails { get; set; } // JSON storing card details, etc.
    public string? FailureReason { get; set; }
    public string? FailureCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? FailedAt { get; set; }

    // For refunds
    public bool IsRefunded { get; set; } = false;
    public decimal? RefundedAmount { get; set; }
    public string? RefundId { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? RefundReason { get; set; }

    // Navigation Properties
    public virtual Order? Order { get; set; }
    public virtual User User { get; set; } = null!;
}
