namespace PetClothingShop.Core.Entities;

/// <summary>
/// Represents a promotional coupon or discount code
/// </summary>
public class Coupon
{
    public int Id { get; set; }

    /// <summary>
    /// Unique coupon code (e.g., "SAVE10", "HOLIDAY20")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of the coupon (e.g., "Get 10% off on all orders")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Discount percentage (0-100)
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Fixed discount amount (if not using percentage)
    /// </summary>
    public decimal? FixedDiscountAmount { get; set; }

    /// <summary>
    /// Maximum discount amount cap
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// Minimum order amount required to use coupon
    /// </summary>
    public decimal MinimumOrderAmount { get; set; }

    /// <summary>
    /// Maximum number of times coupon can be used
    /// </summary>
    public int? MaxUsageCount { get; set; }

    /// <summary>
    /// Maximum number of times per customer
    /// </summary>
    public int? MaxUsagePerCustomer { get; set; }

    /// <summary>
    /// Coupon start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Coupon expiry date
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Is coupon active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Category IDs this coupon applies to (comma-separated)
    /// Leave empty for all categories
    /// </summary>
    public string? ApplicableCategories { get; set; }

    /// <summary>
    /// Product IDs this coupon applies to (comma-separated)
    /// Leave empty for all products
    /// </summary>
    public string? ApplicableProducts { get; set; }

    /// <summary>
    /// Coupon type (Percentage or Fixed)
    /// </summary>
    public string CouponType { get; set; } = "Percentage"; // Percentage or Fixed

    /// <summary>
    /// Current usage count
    /// </summary>
    public int CurrentUsageCount { get; set; } = 0;

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Navigation properties
    /// </summary>
    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}

/// <summary>
/// Tracks coupon usage per customer and order
/// </summary>
public class CouponUsage
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Coupon
    /// </summary>
    public int CouponId { get; set; }

    /// <summary>
    /// Foreign key to User (customer)
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Foreign key to Order
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Discount amount applied
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Used timestamp
    /// </summary>
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation properties
    /// </summary>
    public virtual Coupon Coupon { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Order? Order { get; set; }
}
