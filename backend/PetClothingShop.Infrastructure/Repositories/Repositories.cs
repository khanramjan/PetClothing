using Microsoft.EntityFrameworkCore;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using PetClothingShop.Infrastructure.Data;

namespace PetClothingShop.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Product?> GetByIdWithImagesAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetFeaturedProductsAsync(int count)
    {
        return await _dbSet
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderByDescending(p => p.Rating)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(List<Product> Products, int TotalCount)> GetFilteredProductsAsync(
        string? searchTerm,
        int? categoryId,
        string? petType,
        string? size,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isFeatured,
        string? sortBy,
        int page,
        int pageSize)
    {
        var query = _dbSet
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => 
                p.Name.Contains(searchTerm) || 
                p.Description.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(petType))
        {
            query = query.Where(p => p.PetType == petType);
        }

        if (!string.IsNullOrWhiteSpace(size))
        {
            query = query.Where(p => p.Size == size);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (isFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == isFeatured.Value);
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            "rating" => query.OrderByDescending(p => p.Rating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }
}

public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetCartWithItemsAsync(int userId)
    {
        return await _dbSet
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task ClearCartAsync(int cartId)
    {
        var cart = await _dbSet
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.Id == cartId);

        if (cart != null)
        {
            cart.CartItems.Clear();
            _context.SaveChanges();
        }
    }
}

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
            .Include(o => o.ShippingAddress)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
            .Include(o => o.ShippingAddress)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<(List<Order> Orders, int TotalCount)> GetAllOrdersPagedAsync(int page, int pageSize)
    {
        var totalCount = await _dbSet.CountAsync();
        var orders = await _dbSet
            .Include(o => o.User)
            .Include(o => o.ShippingAddress)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _dbSet.CountAsync(o => o.OrderNumber.StartsWith(date));
        return $"{date}{(count + 1):D4}";
    }
}

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<Category>> GetCategoriesWithSubCategoriesAsync()
    {
        return await _dbSet
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }
}

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<Review>> GetProductReviewsAsync(int productId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
    {
        return await _context.OrderItems
            .AnyAsync(oi => oi.Order.UserId == userId && 
                           oi.ProductId == productId && 
                           oi.Order.PaymentStatus == "Paid");
    }
}

public class AddressRepository : Repository<Address>, IAddressRepository
{
    public AddressRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<Address>> GetUserAddressesAsync(int userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);
    }

    public async Task<List<Payment>> GetOrderPaymentsAsync(int orderId)
    {
        return await _dbSet
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
