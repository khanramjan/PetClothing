namespace PetClothingShop.Core.DTOs;

/// <summary>
/// DTO for creating/updating a coupon
/// </summary>
public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public decimal? FixedDiscountAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public int? MaxUsagePerCustomer { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ApplicableCategories { get; set; }
    public string? ApplicableProducts { get; set; }
    public string CouponType { get; set; } = "Percentage";
}

/// <summary>
/// DTO for displaying coupon information
/// </summary>
public class CouponDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public decimal? FixedDiscountAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public int? MaxUsagePerCustomer { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    public bool IsNotStarted => DateTime.UtcNow < StartDate;
    public decimal RemainingUsagePercentage => MaxUsageCount.HasValue ? ((MaxUsageCount.Value - CurrentUsageCount) / (decimal)MaxUsageCount.Value) * 100 : 100;
    public int CurrentUsageCount { get; set; }
    public string CouponType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for validating coupon before applying
/// </summary>
public class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderSubtotal { get; set; }
    public List<int> CartProductIds { get; set; } = new();
    public List<int> CartCategoryIds { get; set; } = new();
}

/// <summary>
/// DTO for coupon validation response
/// </summary>
public class ValidateCouponResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string CouponCode { get; set; } = string.Empty;
}

/// <summary>
/// DTO for applying coupon during checkout
/// </summary>
public class ApplyCouponRequest
{
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// DTO for shipping method information
/// </summary>
public class ShippingMethodDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BaseCost { get; set; }
    public int MinDeliveryDays { get; set; }
    public int MaxDeliveryDays { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for tax rate information
/// </summary>
public class TaxRateDTO
{
    public int Id { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public decimal TaxPercentage { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for checkout summary
/// </summary>
public class CheckoutSummaryDTO
{
    public List<CartItemDTO> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public List<AddressDTO> UserAddresses { get; set; } = new();
    public List<ShippingMethodDTO> AvailableShippingMethods { get; set; } = new();
}

/// <summary>
/// DTO for creating an order from checkout
/// </summary>
public class CreateOrderFromCheckoutRequest
{
    public int AddressId { get; set; }
    public int ShippingMethodId { get; set; }
    public string? CouponCode { get; set; }
}

/// <summary>
/// DTO for order confirmation
/// </summary>
public class OrderConfirmationDTO
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrderItemDTO> Items { get; set; } = new();
    public AddressDTO ShippingAddress { get; set; } = null!;
    public ShippingMethodDTO ShippingMethod { get; set; } = null!;
    public DateTime EstimatedDelivery { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for tax calculation
/// </summary>
public class TaxCalculationDTO
{
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string StateCode { get; set; } = string.Empty;
}

/// <summary>
/// DTO for shipping calculation
/// </summary>
public class ShippingCalculationDTO
{
    public int ShippingMethodId { get; set; }
    public string ShippingMethodName { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public int MinDeliveryDays { get; set; }
    public int MaxDeliveryDays { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
}

public class ShippingCalculationRequest
{
    public int ShippingMethodId { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public decimal Weight { get; set; } = 5m;
}

public class TaxCalculationRequest
{
    public string StateCode { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
}
