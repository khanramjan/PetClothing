using PetClothingShop.Core.Entities;

namespace PetClothingShop.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}

public interface IProductRepository : IRepository<Product>
{
    Task<(List<Product> Products, int TotalCount)> GetFilteredProductsAsync(
        string? searchTerm,
        int? categoryId,
        string? petType,
        string? size,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isFeatured,
        string? sortBy,
        int page,
        int pageSize);
    Task<List<Product>> GetFeaturedProductsAsync(int count);
    Task<Product?> GetByIdWithImagesAsync(int id);
}

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(int userId);
    Task<Cart?> GetCartWithItemsAsync(int userId);
    Task ClearCartAsync(int cartId);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetUserOrdersAsync(int userId);
    Task<Order?> GetOrderWithDetailsAsync(int orderId);
    Task<(List<Order> Orders, int TotalCount)> GetAllOrdersPagedAsync(int page, int pageSize);
    Task<string> GenerateOrderNumberAsync();
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetCategoriesWithSubCategoriesAsync();
}

public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> GetProductReviewsAsync(int productId);
    Task<bool> HasUserPurchasedProductAsync(int userId, int productId);
}

public interface IAddressRepository : IRepository<Address>
{
    Task<List<Address>> GetUserAddressesAsync(int userId);
}

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);
    Task<List<Coupon>> GetActiveCouponsAsync();
    Task<Coupon?> GetByCodeWithUsageAsync(string code, int userId);
    Task RecordCouponUsageAsync(CouponUsage usage);
    Task<int> GetCouponUsageCountAsync(int couponId);
    Task<int> GetCouponUsageCountByUserAsync(int couponId, int userId);
}

public interface IShippingMethodRepository : IRepository<ShippingMethod>
{
    Task<List<ShippingMethod>> GetActiveShippingMethodsAsync();
    Task<ShippingMethod?> GetByNameAsync(string name);
}

public interface ITaxRateRepository : IRepository<TaxRate>
{
    Task<TaxRate?> GetByStateCodeAsync(string stateCode);
    Task<List<TaxRate>> GetActiveRatesAsync();
}

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId);
    Task<List<Payment>> GetOrderPaymentsAsync(int orderId);
}


