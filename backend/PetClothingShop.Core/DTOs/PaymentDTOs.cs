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
    public int? OrderId { get; set; }
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

/// <summary>
/// Request to initiate an SSLCommerz payment gateway session
/// </summary>
public class InitiatePaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerState { get; set; } = string.Empty;
    public string CustomerPostcode { get; set; } = string.Empty;
    public string CustomerCountry { get; set; } = "Bangladesh";
    public string Description { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string FailUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response containing SSLCommerz gateway details
/// </summary>
public class InitiatePaymentResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string GatewayPageURL { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// SSLCommerz payment validation callback
/// </summary>
/// <summary>
/// SSLCommerz callback/validation request - property names must match exactly what SSLCommerz sends
/// Note: SSLCommerz sends data with inconsistent casing - some fields are lowercase, some are uppercase
/// All fields are nullable since SSLCommerz may not send all fields in every callback
/// </summary>
public class SSLCommerzValidationRequest
{
    // Primary fields - these come in the callback
    public string? tran_id { get; set; }
    public string? val_id { get; set; }
    public string? amount { get; set; }
    public string? card_type { get; set; }
    public string? store_amount { get; set; }
    public string? card_no { get; set; }
    public string? bank_tran_id { get; set; }
    public string? status { get; set; } // VALID, INVALID, FAILED, CANCELLED
    public string? tran_date { get; set; }
    public string? error { get; set; }
    public string? currency { get; set; }
    public string? card_issuer { get; set; }
    public string? card_brand { get; set; }
    public string? card_sub_brand { get; set; }
    public string? card_issuer_country { get; set; }
    public string? card_issuer_country_code { get; set; }
    public string? store_id { get; set; }
    public string? verify_sign { get; set; }
    public string? verify_key { get; set; }
    public string? verify_sign_sha2 { get; set; }
    public string? currency_type { get; set; }
    public string? currency_amount { get; set; }
    public string? currency_rate { get; set; }
    public string? base_fair { get; set; }
    public string? value_a { get; set; }
    public string? value_b { get; set; }
    public string? value_c { get; set; }
    public string? value_d { get; set; }
    public string? risk_level { get; set; }
    public string? risk_title { get; set; }
    
    // Helper properties for cleaner code
    public string TransactionId => tran_id ?? string.Empty;
    public string Status => status ?? string.Empty;
    public string ValidationId => val_id ?? string.Empty;
    public string Amount => amount ?? string.Empty;
}

/// <summary>
/// SSLCommerz API request to initiate a payment session
/// </summary>
public class SSLCommerzSessionRequest
{
    public string store_id { get; set; } = string.Empty;
    public string store_passwd { get; set; } = string.Empty;
    public string total_amount { get; set; } = string.Empty;
    public string currency { get; set; } = "BDT";
    public string tran_id { get; set; } = string.Empty;
    public string success_url { get; set; } = string.Empty;
    public string fail_url { get; set; } = string.Empty;
    public string cancel_url { get; set; } = string.Empty;
    
    // Customer Information
    public string cus_name { get; set; } = string.Empty;
    public string cus_email { get; set; } = string.Empty;
    public string cus_phone { get; set; } = string.Empty;
    public string cus_add1 { get; set; } = string.Empty;
    public string cus_city { get; set; } = string.Empty;
    public string cus_postcode { get; set; } = string.Empty;
    public string cus_state { get; set; } = string.Empty;
    public string cus_country { get; set; } = "Bangladesh";
    
    // Shipping Information
    public string shipping_method { get; set; } = "YES";
    public string ship_name { get; set; } = string.Empty;
    public string ship_add1 { get; set; } = string.Empty;
    public string ship_city { get; set; } = string.Empty;
    public string ship_state { get; set; } = string.Empty;
    public string ship_postcode { get; set; } = string.Empty;
    public string ship_country { get; set; } = "Bangladesh";
    
    // Product Information
    public string product_name { get; set; } = string.Empty;
    public string product_category { get; set; } = string.Empty;
    public string product_profile { get; set; } = "general";
    
    // Optional custom fields - SSLCommerz sends these back in callbacks
    public string? value_a { get; set; }
    public string? value_b { get; set; }
    public string? value_c { get; set; }
    public string? value_d { get; set; }
}

/// <summary>
/// SSLCommerz API response containing session key
/// </summary>
public class SSLCommerzSessionResponse
{
    public string status { get; set; } = string.Empty;
    public string sessionkey { get; set; } = string.Empty;
    public string gateway_url { get; set; } = string.Empty;
    public string redirectGatewayURL { get; set; } = string.Empty;
    public string GatewayPageURL { get; set; } = string.Empty;
    public string errorMessage { get; set; } = string.Empty;
    public string failedreason { get; set; } = string.Empty;
}

