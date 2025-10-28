namespace PetClothingShop.Core.DTOs;

/// <summary>
/// Request to create a Stripe Payment Intent
/// </summary>
public class CreatePaymentIntentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string? Email { get; set; }
    public string? ReceiptEmail { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Response containing Stripe Payment Intent details
/// </summary>
public class PaymentIntentResponse
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // requires_payment_method, processing, succeeded, etc.
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to confirm a Stripe payment
/// </summary>
public class ConfirmPaymentRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string PaymentMethodId { get; set; } = string.Empty;
    public int OrderId { get; set; }
}

/// <summary>
/// Response after payment confirmation
/// </summary>
public class PaymentConfirmationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? OrderId { get; set; }
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Webhook payload from Stripe
/// </summary>
public class StripeWebhookRequest
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Payment history entry
/// </summary>
public class PaymentHistoryDTO
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
}

/// <summary>
/// Refund request
/// </summary>
public class RefundRequest
{
    public int OrderId { get; set; }
    public string? Reason { get; set; }
    public decimal? Amount { get; set; }
}

/// <summary>
/// Refund response
/// </summary>
public class RefundResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RefundId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
}
