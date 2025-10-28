namespace PetClothingShop.Core.Entities;

/// <summary>
/// Represents shipping methods available for delivery
/// </summary>
public class ShippingMethod
{
    public int Id { get; set; }

    /// <summary>
    /// Name of shipping method (e.g., "Standard", "Express", "Overnight")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the shipping method
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Base shipping cost
    /// </summary>
    public decimal BaseCost { get; set; }

    /// <summary>
    /// Cost per pound/kg (for variable weight-based pricing)
    /// </summary>
    public decimal? CostPerWeight { get; set; }

    /// <summary>
    /// Expected delivery days (minimum)
    /// </summary>
    public int MinDeliveryDays { get; set; }

    /// <summary>
    /// Expected delivery days (maximum)
    /// </summary>
    public int MaxDeliveryDays { get; set; }

    /// <summary>
    /// Whether this shipping method is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Supported regions (comma-separated state codes, empty = all)
    /// </summary>
    public string? SupportedRegions { get; set; }

    /// <summary>
    /// Maximum order weight this method supports (in kg)
    /// </summary>
    public decimal? MaxWeight { get; set; }

    /// <summary>
    /// Carrier name if applicable (e.g., "FedEx", "UPS", "Local Courier")
    /// </summary>
    public string? CarrierName { get; set; }

    /// <summary>
    /// Track URL pattern for this carrier (e.g., "https://track.fedex.com/{tracking_number}")
    /// </summary>
    public string? TrackingUrlPattern { get; set; }

    /// <summary>
    /// API integration name if using third-party (e.g., "pathao", "redx")
    /// </summary>
    public string? IntegrationProvider { get; set; }

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
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

/// <summary>
/// Represents tax rates by state/region
/// </summary>
public class TaxRate
{
    public int Id { get; set; }

    /// <summary>
    /// State/region code (e.g., "CA", "NY", "TX")
    /// </summary>
    public string StateCode { get; set; } = string.Empty;

    /// <summary>
    /// State/region name
    /// </summary>
    public string StateName { get; set; } = string.Empty;

    /// <summary>
    /// Tax rate percentage
    /// </summary>
    public decimal TaxPercentage { get; set; }

    /// <summary>
    /// Is this tax rate active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
