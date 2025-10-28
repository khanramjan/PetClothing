using Microsoft.EntityFrameworkCore;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using PetClothingShop.Infrastructure.Data;

namespace PetClothingShop.Infrastructure.Repositories;

public class ShippingMethodRepository : Repository<ShippingMethod>, IShippingMethodRepository
{
    private readonly ApplicationDbContext _context;

    public ShippingMethodRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<ShippingMethod>> GetActiveShippingMethodsAsync()
    {
        return await _context.ShippingMethods
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    public async Task<ShippingMethod?> GetByNameAsync(string name)
    {
        return await _context.ShippingMethods
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
    }
}
