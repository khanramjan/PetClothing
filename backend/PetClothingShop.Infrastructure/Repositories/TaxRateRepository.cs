using Microsoft.EntityFrameworkCore;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using PetClothingShop.Infrastructure.Data;

namespace PetClothingShop.Infrastructure.Repositories;

public class TaxRateRepository : Repository<TaxRate>, ITaxRateRepository
{
    private readonly ApplicationDbContext _context;

    public TaxRateRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<TaxRate?> GetByStateCodeAsync(string stateCode)
    {
        return await _context.TaxRates
            .FirstOrDefaultAsync(t => t.StateCode.ToUpper() == stateCode.ToUpper() && t.IsActive);
    }

    public async Task<List<TaxRate>> GetActiveRatesAsync()
    {
        return await _context.TaxRates
            .Where(t => t.IsActive)
            .OrderBy(t => t.StateName)
            .ToListAsync();
    }
}
