using PetClothingShop.Core.DTOs;

namespace PetClothingShop.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> RevokeTokenAsync(int userId);
}

public interface IProductService
{
    Task<PagedResult<ProductDTO>> GetProductsAsync(ProductFilterRequest filter);
    Task<ProductDTO?> GetProductByIdAsync(int id);
    Task<ProductDTO> CreateProductAsync(CreateProductRequest request);
    Task<ProductDTO> UpdateProductAsync(UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
    Task<List<ProductDTO>> GetFeaturedProductsAsync(int count = 8);
}

public interface ICartService
{
    Task<CartDTO> GetCartAsync(int userId);
    Task<CartDTO> AddToCartAsync(int userId, AddToCartRequest request);
    Task<CartDTO> UpdateCartItemAsync(int userId, UpdateCartItemRequest request);
    Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
    Task<bool> ClearCartAsync(int userId);
}

public interface IOrderService
{
    Task<OrderDTO> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<OrderDTO?> GetOrderByIdAsync(int userId, int orderId);
    Task<List<OrderDTO>> GetUserOrdersAsync(int userId);
    Task<PagedResult<OrderDTO>> GetAllOrdersAsync(int page = 1, int pageSize = 20);
    Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusRequest request);
    Task<bool> CancelOrderAsync(int userId, int orderId);
}

public interface ICategoryService
{
    Task<List<CategoryDTO>> GetCategoriesAsync();
    Task<CategoryDTO?> GetCategoryByIdAsync(int id);
    Task<CategoryDTO> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryDTO> UpdateCategoryAsync(int id, CreateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int id);
}

public interface IReviewService
{
    Task<List<ReviewDTO>> GetProductReviewsAsync(int productId);
    Task<ReviewDTO> CreateReviewAsync(int userId, CreateReviewRequest request);
    Task<bool> DeleteReviewAsync(int userId, int reviewId);
    Task<bool> ApproveReviewAsync(int reviewId);
}

public interface IUserService
{
    Task<List<AddressDTO>> GetUserAddressesAsync(int userId);
    Task<AddressDTO> AddAddressAsync(int userId, CreateAddressRequest request);
    Task<AddressDTO> UpdateAddressAsync(int userId, int addressId, CreateAddressRequest request);
    Task<bool> DeleteAddressAsync(int userId, int addressId);
    Task<bool> SetDefaultAddressAsync(int userId, int addressId);
}

public interface IDashboardService
{
    Task<DashboardStatsDTO> GetDashboardStatsAsync();
}

public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(int userId, CreatePaymentIntentRequest request);
    Task<PaymentConfirmationResponse> ConfirmPaymentAsync(int userId, ConfirmPaymentRequest request);
    Task<bool> HandleStripeWebhookAsync(string json, string signature);
    Task<RefundResponse> RefundPaymentAsync(int userId, RefundRequest request);
    Task<PaymentHistoryDTO?> GetPaymentAsync(int paymentId);
    Task<List<PaymentHistoryDTO>> GetUserPaymentHistoryAsync(int userId);
    Task<InitiatePaymentResponse> InitiateSSLCommerzPaymentAsync(InitiatePaymentRequest request, int userId);
    Task<bool> ValidateSSLCommerzPaymentAsync(SSLCommerzValidationRequest request);
    Task HandleSSLCommerzFailureAsync(SSLCommerzValidationRequest request);
    Task HandleSSLCommerzCancellationAsync(SSLCommerzValidationRequest request);
}

public interface ICouponService
{
    Task<CouponDTO> CreateCouponAsync(CreateCouponRequest request);
    Task<CouponDTO?> GetCouponByCodeAsync(string code);
    Task<CouponDTO?> GetCouponByIdAsync(int id);
    Task<List<CouponDTO>> GetAllCouponsAsync();
    Task<List<CouponDTO>> GetActiveCouponsAsync();
    Task<CouponDTO> UpdateCouponAsync(int id, CreateCouponRequest request);
    Task<bool> DeleteCouponAsync(int id);
    Task<ValidateCouponResponse> ValidateCouponAsync(ValidateCouponRequest request);
    Task<ValidateCouponResponse> ApplyCouponAsync(int userId, string couponCode, decimal orderSubtotal, List<int> productIds);
    Task<bool> RecordCouponUsageAsync(int couponId, int userId, int? orderId, decimal discountAmount);
}

public interface ICheckoutService
{
    Task<CheckoutSummaryDTO> GetCheckoutSummaryAsync(int userId);
    Task<OrderConfirmationDTO> CreateOrderAsync(int userId, CreateOrderFromCheckoutRequest request);
    Task<TaxCalculationDTO> CalculateTaxAsync(string stateCode, decimal subtotal);
    Task<ShippingCalculationDTO> CalculateShippingAsync(int shippingMethodId, string stateCode, decimal weight = 5m);
    Task<List<ShippingMethodDTO>> GetAvailableShippingMethodsAsync();
    Task<List<TaxRateDTO>> GetTaxRatesAsync();
}

public interface IShippingService
{
    Task<List<ShippingMethodDTO>> GetShippingMethodsAsync();
    Task<ShippingMethodDTO?> GetShippingMethodByIdAsync(int id);
    Task<ShippingMethodDTO> CreateShippingMethodAsync(ShippingMethodDTO request);
    Task<ShippingMethodDTO> UpdateShippingMethodAsync(int id, ShippingMethodDTO request);
    Task<bool> DeleteShippingMethodAsync(int id);
    Task<ShippingCalculationDTO> CalculateShippingCostAsync(int shippingMethodId, decimal weight, string stateCode);
}

public interface ITaxService
{
    Task<List<TaxRateDTO>> GetTaxRatesAsync();
    Task<TaxRateDTO?> GetTaxRateByStateAsync(string stateCode);
    Task<TaxRateDTO> CreateTaxRateAsync(TaxRateDTO request);
    Task<TaxRateDTO> UpdateTaxRateAsync(int id, TaxRateDTO request);
    Task<bool> DeleteTaxRateAsync(int id);
    Task<TaxCalculationDTO> CalculateTaxAsync(string stateCode, decimal subtotal);
}
