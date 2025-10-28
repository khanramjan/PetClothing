using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace PetClothingShop.Infrastructure.Services;

public class CheckoutService : ICheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICouponService _couponService;
    private readonly ITaxService _taxService;
    private readonly IShippingService _shippingService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ICouponService couponService,
        ITaxService taxService,
        IShippingService shippingService,
        IOrderRepository orderRepository,
        ILogger<CheckoutService> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _couponService = couponService;
        _taxService = taxService;
        _shippingService = shippingService;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<CheckoutSummaryDTO> GetCheckoutSummaryAsync(int userId)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart?.CartItems == null || cart.CartItems.Count == 0)
            throw new InvalidOperationException("Cart is empty");

        decimal subtotal = 0;
        var items = new List<CartItemDTO>();

        foreach (var item in cart.CartItems)
        {
            var product = item.Product;
            if (product == null)
                continue;

            var itemTotal = product.Price * item.Quantity;
            subtotal += itemTotal;

            items.Add(new CartItemDTO
            {
                Id = item.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = item.Quantity,
                Subtotal = itemTotal
            });
        }

        return new CheckoutSummaryDTO
        {
            Items = items,
            Subtotal = subtotal
        };
    }

    public async Task<OrderConfirmationDTO> CreateOrderAsync(int userId, CreateOrderFromCheckoutRequest request)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart?.CartItems == null || cart.CartItems.Count == 0)
            throw new InvalidOperationException("Cart is empty");

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();
        var productIds = new List<int>();

        foreach (var item in cart.CartItems)
        {
            var product = item.Product;
            if (product == null)
                continue;

            var itemTotal = product.Price * item.Quantity;
            subtotal += itemTotal;
            productIds.Add(product.Id);

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSKU = product.SKU,
                Price = product.Price,
                Quantity = item.Quantity,
                Subtotal = itemTotal
            });
        }

        decimal discountAmount = 0;
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            try
            {
                var couponResult = await _couponService.ApplyCouponAsync(userId, request.CouponCode, subtotal, productIds);
                if (couponResult.IsValid)
                {
                    discountAmount = couponResult.DiscountAmount;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error applying coupon: {ex.Message}");
            }
        }

        var subtotalAfterDiscount = subtotal - discountAmount;
        var taxCalculation = await _taxService.CalculateTaxAsync("US", subtotalAfterDiscount);
        var shippingCalculation = await _shippingService.CalculateShippingCostAsync(request.ShippingMethodId, 5m, "US");

        var total = subtotalAfterDiscount + taxCalculation.TaxAmount + shippingCalculation.ShippingCost;

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending",
            SubTotal = subtotal,
            Tax = taxCalculation.TaxAmount,
            ShippingCost = shippingCalculation.ShippingCost,
            Total = total,
            ShippingAddressId = request.AddressId,
            OrderItems = orderItems,
            PaymentMethod = "stripe",
            PaymentStatus = "Pending"
        };

        await _orderRepository.AddAsync(order);
        await _cartRepository.ClearCartAsync(cart.Id);

        _logger.LogInformation($"Order created: {order.Id} with total ${total}");

        return new OrderConfirmationDTO
        {
            OrderId = order.Id,
            OrderNumber = $"ORD-{order.Id:D6}",
            Total = order.Total,
            EstimatedDelivery = shippingCalculation.EstimatedDeliveryDate,
            CreatedAt = order.CreatedAt
        };
    }

    public async Task<TaxCalculationDTO> CalculateTaxAsync(string stateCode, decimal subtotal)
    {
        return await _taxService.CalculateTaxAsync(stateCode, subtotal);
    }

    public async Task<ShippingCalculationDTO> CalculateShippingAsync(int shippingMethodId, string stateCode, decimal weight = 5m)
    {
        return await _shippingService.CalculateShippingCostAsync(shippingMethodId, weight, stateCode);
    }

    public async Task<List<ShippingMethodDTO>> GetAvailableShippingMethodsAsync()
    {
        return await _shippingService.GetShippingMethodsAsync();
    }

    public async Task<List<TaxRateDTO>> GetTaxRatesAsync()
    {
        return await _taxService.GetTaxRatesAsync();
    }
}
