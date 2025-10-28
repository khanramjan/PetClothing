using Microsoft.EntityFrameworkCore;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using PetClothingShop.Infrastructure.Data;

namespace PetClothingShop.Infrastructure.Repositories;

public class CouponRepository : Repository<Coupon>, ICouponRepository
{
    private readonly ApplicationDbContext _context;

    public CouponRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task<List<Coupon>> GetActiveCouponsAsync()
    {
        return await _context.Coupons
            .Where(c => c.IsActive && c.StartDate <= DateTime.UtcNow && c.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<Coupon?> GetByCodeWithUsageAsync(string code, int userId)
    {
        return await _context.Coupons
            .Include(c => c.CouponUsages.Where(cu => cu.UserId == userId))
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task RecordCouponUsageAsync(CouponUsage usage)
    {
        _context.CouponUsages.Add(usage);
        
        var coupon = await _context.Coupons.FindAsync(usage.CouponId);
        if (coupon != null)
        {
            coupon.CurrentUsageCount++;
            _context.Coupons.Update(coupon);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCouponUsageCountAsync(int couponId)
    {
        return await _context.CouponUsages
            .CountAsync(cu => cu.CouponId == couponId);
    }

    public async Task<int> GetCouponUsageCountByUserAsync(int couponId, int userId)
    {
        return await _context.CouponUsages
            .CountAsync(cu => cu.CouponId == couponId && cu.UserId == userId);
    }
}
