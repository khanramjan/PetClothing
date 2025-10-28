using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace PetClothingShop.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _couponRepository;
    private readonly ILogger<CouponService> _logger;

    public CouponService(
        ICouponRepository couponRepository,
        ILogger<CouponService> logger)
    {
        _couponRepository = couponRepository;
        _logger = logger;
    }

    public async Task<CouponDTO> CreateCouponAsync(CreateCouponRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Coupon code is required");

        if (request.ExpiryDate <= DateTime.UtcNow)
            throw new ArgumentException("Expiry date must be in the future");

        var existing = await _couponRepository.GetByCodeAsync(request.Code);
        if (existing != null)
            throw new InvalidOperationException("Coupon code already exists");

        var coupon = new Coupon
        {
            Code = request.Code.ToUpper(),
            Description = request.Description,
            DiscountPercentage = request.DiscountPercentage,
            FixedDiscountAmount = request.FixedDiscountAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaxUsageCount = request.MaxUsageCount,
            MaxUsagePerCustomer = request.MaxUsagePerCustomer,
            StartDate = request.StartDate,
            ExpiryDate = request.ExpiryDate,
            IsActive = request.IsActive,
            ApplicableCategories = request.ApplicableCategories,
            ApplicableProducts = request.ApplicableProducts,
            CouponType = request.CouponType,
            CreatedAt = DateTime.UtcNow
        };

        await _couponRepository.AddAsync(coupon);
        _logger.LogInformation($"Coupon created: {coupon.Code}");

        return MapToDTO(coupon);
    }

    public async Task<CouponDTO?> GetCouponByCodeAsync(string code)
    {
        var coupon = await _couponRepository.GetByCodeAsync(code);
        return coupon != null ? MapToDTO(coupon) : null;
    }

    public async Task<CouponDTO?> GetCouponByIdAsync(int id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        return coupon != null ? MapToDTO(coupon) : null;
    }

    public async Task<List<CouponDTO>> GetAllCouponsAsync()
    {
        var coupons = await _couponRepository.GetAllAsync();
        return coupons.Select(MapToDTO).ToList();
    }

    public async Task<List<CouponDTO>> GetActiveCouponsAsync()
    {
        var coupons = await _couponRepository.GetActiveCouponsAsync();
        return coupons.Select(MapToDTO).ToList();
    }

    public async Task<CouponDTO> UpdateCouponAsync(int id, CreateCouponRequest request)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null)
            throw new InvalidOperationException("Coupon not found");

        coupon.Description = request.Description;
        coupon.DiscountPercentage = request.DiscountPercentage;
        coupon.FixedDiscountAmount = request.FixedDiscountAmount;
        coupon.MaxDiscountAmount = request.MaxDiscountAmount;
        coupon.MinimumOrderAmount = request.MinimumOrderAmount;
        coupon.MaxUsageCount = request.MaxUsageCount;
        coupon.MaxUsagePerCustomer = request.MaxUsagePerCustomer;
        coupon.ExpiryDate = request.ExpiryDate;
        coupon.IsActive = request.IsActive;
        coupon.UpdatedAt = DateTime.UtcNow;

        await _couponRepository.UpdateAsync(coupon);
        _logger.LogInformation($"Coupon updated: {coupon.Code}");

        return MapToDTO(coupon);
    }

    public async Task<bool> DeleteCouponAsync(int id)
    {
        var deleted = await _couponRepository.DeleteAsync(id);
        if (deleted)
            _logger.LogInformation($"Coupon deleted: {id}");
        return deleted;
    }

    public async Task<ValidateCouponResponse> ValidateCouponAsync(ValidateCouponRequest request)
    {
        var coupon = await _couponRepository.GetByCodeAsync(request.Code);

        if (coupon == null)
            return new ValidateCouponResponse 
            { 
                IsValid = false, 
                Message = "Invalid coupon code" 
            };

        if (!coupon.IsActive)
            return new ValidateCouponResponse 
            { 
                IsValid = false, 
                Message = "This coupon is no longer active" 
            };

        if (DateTime.UtcNow > coupon.ExpiryDate)
            return new ValidateCouponResponse 
            { 
                IsValid = false, 
                Message = "This coupon has expired" 
            };

        if (DateTime.UtcNow < coupon.StartDate)
            return new ValidateCouponResponse 
            { 
                IsValid = false, 
                Message = "This coupon is not yet active" 
            };

        if (request.OrderSubtotal < coupon.MinimumOrderAmount)
            return new ValidateCouponResponse 
            { 
                IsValid = false, 
                Message = $"Minimum order amount of ${coupon.MinimumOrderAmount} required" 
            };

        if (coupon.MaxUsageCount.HasValue && coupon.CurrentUsageCount >= coupon.MaxUsageCount)
            return new ValidateCouponResponse 
            { 
                IsValid = false, 
                Message = "This coupon has reached its usage limit" 
            };

        decimal discountAmount = 0;
        decimal discountPercentage = 0;

        if (coupon.CouponType == "Percentage")
        {
            discountPercentage = coupon.DiscountPercentage;
            discountAmount = (request.OrderSubtotal * coupon.DiscountPercentage) / 100;

            if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount)
                discountAmount = coupon.MaxDiscountAmount.Value;
        }
        else if (coupon.CouponType == "Fixed")
        {
            discountAmount = coupon.FixedDiscountAmount ?? 0;
            discountPercentage = (discountAmount / request.OrderSubtotal) * 100;
        }

        return new ValidateCouponResponse
        {
            IsValid = true,
            Message = "Coupon is valid",
            DiscountAmount = discountAmount,
            DiscountPercentage = discountPercentage,
            CouponCode = coupon.Code
        };
    }

    public async Task<ValidateCouponResponse> ApplyCouponAsync(int userId, string couponCode, decimal orderSubtotal, List<int> productIds)
    {
        var coupon = await _couponRepository.GetByCodeWithUsageAsync(couponCode, userId);

        if (coupon == null)
            return new ValidateCouponResponse 
            { 
                IsValid = false, 
                Message = "Invalid coupon code" 
            };

        var validateRequest = new ValidateCouponRequest
        {
            Code = couponCode,
            OrderSubtotal = orderSubtotal,
            CartProductIds = productIds
        };

        return await ValidateCouponAsync(validateRequest);
    }

    public async Task<bool> RecordCouponUsageAsync(int couponId, int userId, int? orderId, decimal discountAmount)
    {
        try
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId);
            if (coupon == null)
                return false;

            if (coupon.MaxUsagePerCustomer.HasValue)
            {
                var userUsageCount = await _couponRepository.GetCouponUsageCountByUserAsync(couponId, userId);
                if (userUsageCount >= coupon.MaxUsagePerCustomer)
                    return false;
            }

            var usage = new CouponUsage
            {
                CouponId = couponId,
                UserId = userId,
                OrderId = orderId,
                DiscountAmount = discountAmount,
                UsedAt = DateTime.UtcNow
            };

            await _couponRepository.RecordCouponUsageAsync(usage);
            _logger.LogInformation($"Coupon usage recorded: Coupon {couponId}, User {userId}, Order {orderId}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording coupon usage");
            return false;
        }
    }

    private CouponDTO MapToDTO(Coupon coupon)
    {
        return new CouponDTO
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            DiscountPercentage = coupon.DiscountPercentage,
            FixedDiscountAmount = coupon.FixedDiscountAmount,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            MinimumOrderAmount = coupon.MinimumOrderAmount,
            MaxUsageCount = coupon.MaxUsageCount,
            MaxUsagePerCustomer = coupon.MaxUsagePerCustomer,
            StartDate = coupon.StartDate,
            ExpiryDate = coupon.ExpiryDate,
            IsActive = coupon.IsActive,
            CurrentUsageCount = coupon.CurrentUsageCount,
            CouponType = coupon.CouponType,
            CreatedAt = coupon.CreatedAt
        };
    }
}
