# 🚀 PHASE 1 IMPLEMENTATION GUIDE - Critical Features (7 Days)

**Objective:** Enable revenue generation with complete checkout, coupons, tax, shipping, and email notifications

**Target Date:** 7 days from start  
**Estimated Effort:** 40-50 development hours

---

## 📋 Overview

### Phase 1 Deliverables
1. ✅ **Complete Checkout Flow** (Days 1-2)
   - Shopping cart review
   - Shipping address selection
   - Shipping method selection
   - Tax and shipping cost calculation
   - Stripe payment integration (frontend)
   - Order confirmation

2. ✅ **Coupon/Promo System** (Day 3)
   - Backend: Coupon entity, service, CRUD
   - Frontend: Apply coupon UI
   - Discount calculation

3. ✅ **Email Notifications** (Day 4)
   - SendGrid integration
   - Order confirmation emails
   - Shipment notifications
   - Customer support emails

4. ✅ **Tax Calculation** (Day 5)
   - Tax calculation service
   - Tax rate configuration
   - Invoice generation

5. ✅ **Shipping Integration** (Day 6)
   - Shipping options (Standard, Express, Overnight)
   - Real-time shipping cost calculation
   - Delivery time estimation

6. ✅ **Order Tracking** (Day 7)
   - Customer order tracking page
   - Order status updates
   - Delivery tracking

---

## 🛠️ Implementation Tasks

### TASK 1: Complete Checkout Page (Days 1-2)

#### Step 1.1: Create Checkout Backend Service

**File:** `backend/PetClothingShop.Infrastructure/Services/CheckoutService.cs`

```csharp
using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PetClothingShop.Infrastructure.Services;

public interface ICheckoutService
{
    Task<CheckoutSummaryDTO> GetCheckoutSummaryAsync(int userId);
    Task<OrderDTO> CreateOrderFromCheckoutAsync(int userId, CheckoutRequest request);
    Task<TaxCalculationDTO> CalculateTaxAsync(string state, decimal subtotal);
    Task<ShippingCalculationDTO> CalculateShippingAsync(string state, string city, decimal weight, string shippingMethod);
}

public class CheckoutService : ICheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;

    public CheckoutService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IAddressRepository addressRepository,
        IProductRepository productRepository,
        ICouponRepository couponRepository)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _addressRepository = addressRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
    }

    public async Task<CheckoutSummaryDTO> GetCheckoutSummaryAsync(int userId)
    {
        var cart = await _cartRepository.GetUserCartAsync(userId);
        var addresses = await _addressRepository.GetUserAddressesAsync(userId);

        var items = cart.CartItems.Select(ci => new CartItemDTO
        {
            Id = ci.Id,
            ProductId = ci.Product.Id,
            ProductName = ci.Product.Name,
            Quantity = ci.Quantity,
            Price = ci.Product.DiscountPrice ?? ci.Product.Price,
            Total = (ci.Product.DiscountPrice ?? ci.Product.Price) * ci.Quantity,
            Image = ci.Product.ProductImages.FirstOrDefault()?.ImageUrl ?? ""
        }).ToList();

        decimal subtotal = items.Sum(i => i.Total);

        return new CheckoutSummaryDTO
        {
            Items = items,
            Subtotal = subtotal,
            UserAddresses = addresses.Select(a => new AddressDTO { /* map */ }).ToList(),
            ShippingMethods = GetAvailableShippingMethods()
        };
    }

    public async Task<OrderDTO> CreateOrderFromCheckoutAsync(int userId, CheckoutRequest request)
    {
        // Validate cart
        var cart = await _cartRepository.GetUserCartAsync(userId);
        if (!cart.CartItems.Any())
            throw new InvalidOperationException("Cart is empty");

        // Validate address
        var address = await _addressRepository.GetAddressByIdAsync(request.AddressId);
        if (address == null || address.UserId != userId)
            throw new InvalidOperationException("Invalid address");

        // Calculate totals
        var subtotal = cart.CartItems.Sum(ci => (ci.Product.DiscountPrice ?? ci.Product.Price) * ci.Quantity);
        var taxInfo = await CalculateTaxAsync(address.State, subtotal);
        var shippingInfo = await CalculateShippingAsync(address.State, address.City, 5, request.ShippingMethod);

        decimal total = subtotal + taxInfo.TaxAmount + shippingInfo.ShippingCost;

        // Apply coupon if provided
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(request.CouponCode);
            if (coupon != null && coupon.IsActive && coupon.ExpiryDate > DateTime.UtcNow)
            {
                decimal discount = (subtotal * coupon.DiscountPercentage) / 100;
                total -= discount;
            }
        }

        // Create order
        var order = new Order
        {
            UserId = userId,
            AddressId = request.AddressId,
            Subtotal = subtotal,
            TaxAmount = taxInfo.TaxAmount,
            ShippingCost = shippingInfo.ShippingCost,
            Total = total,
            Status = "Pending",
            PaymentStatus = "Pending",
            ShippingMethod = request.ShippingMethod,
            EstimatedDelivery = DateTime.UtcNow.AddDays(shippingInfo.DeliveryDays),
            CreatedAt = DateTime.UtcNow,
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product.DiscountPrice ?? ci.Product.Price
            }).ToList()
        };

        await _orderRepository.AddAsync(order);

        // Clear cart
        await _cartRepository.ClearCartAsync(userId);

        return new OrderDTO { /* map from order */ };
    }

    public async Task<TaxCalculationDTO> CalculateTaxAsync(string state, decimal subtotal)
    {
        // Tax rates by state (simplified - you can make this configurable)
        var taxRates = new Dictionary<string, decimal>
        {
            { "CA", 0.0725m },
            { "NY", 0.0400m },
            { "TX", 0.0625m },
            { "FL", 0.0600m }
        };

        decimal taxRate = taxRates.ContainsKey(state) ? taxRates[state] : 0.05m;
        decimal taxAmount = subtotal * taxRate;

        return new TaxCalculationDTO
        {
            TaxRate = taxRate,
            TaxAmount = taxAmount,
            State = state
        };
    }

    public async Task<ShippingCalculationDTO> CalculateShippingAsync(string state, string city, decimal weight, string shippingMethod)
    {
        // Simplified shipping calculation
        var shippingRates = new Dictionary<string, (decimal baseCost, int days)>
        {
            { "Standard", (5.99m, 5) },
            { "Express", (12.99m, 3) },
            { "Overnight", (24.99m, 1) }
        };

        if (shippingRates.TryGetValue(shippingMethod, out var rate))
        {
            return new ShippingCalculationDTO
            {
                ShippingMethod = shippingMethod,
                ShippingCost = rate.baseCost,
                DeliveryDays = rate.days,
                EstimatedDelivery = DateTime.UtcNow.AddDays(rate.days)
            };
        }

        throw new InvalidOperationException("Invalid shipping method");
    }

    private List<ShippingMethodDTO> GetAvailableShippingMethods()
    {
        return new List<ShippingMethodDTO>
        {
            new ShippingMethodDTO { Name = "Standard", Cost = 5.99m, DeliveryDays = 5 },
            new ShippingMethodDTO { Name = "Express", Cost = 12.99m, DeliveryDays = 3 },
            new ShippingMethodDTO { Name = "Overnight", Cost = 24.99m, DeliveryDays = 1 }
        };
    }
}
```

#### Step 1.2: Create Checkout DTOs

**File:** `backend/PetClothingShop.Core/DTOs/CheckoutDTOs.cs` (Add to existing DTOs)

```csharp
namespace PetClothingShop.Core.DTOs;

public class CheckoutSummaryDTO
{
    public List<CartItemDTO> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public List<AddressDTO> UserAddresses { get; set; } = new();
    public List<ShippingMethodDTO> ShippingMethods { get; set; } = new();
}

public class CheckoutRequest
{
    public int AddressId { get; set; }
    public string ShippingMethod { get; set; } = "Standard";
    public string? CouponCode { get; set; }
}

public class TaxCalculationDTO
{
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string State { get; set; } = string.Empty;
}

public class ShippingCalculationDTO
{
    public string ShippingMethod { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public int DeliveryDays { get; set; }
    public DateTime EstimatedDelivery { get; set; }
}

public class ShippingMethodDTO
{
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int DeliveryDays { get; set; }
}
```

#### Step 1.3: Update Program.cs to Register Services

Add this to the services registration section in `Program.cs`:

```csharp
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
```

#### Step 1.4: Create Frontend Checkout Page

**File:** `frontend/src/pages/Checkout.tsx`

This will be a 4-step process:
1. Review Cart Items
2. Select/Add Shipping Address
3. Select Shipping Method (see costs)
4. Payment Information

---

### TASK 2: Coupon System (Day 3)

**Requirements:**
- Create Coupon entity
- Create CouponService
- Add API endpoints
- Frontend validation

---

### TASK 3: Email Notifications (Day 4)

**Requirements:**
- SendGrid integration
- Email templates
- Trigger on order creation

---

### TASK 4: Tax Calculation (Day 5)

**Requirements:**
- Tax rates by state
- Tax calculation service
- Include in checkout

---

### TASK 5: Shipping Integration (Day 6)

**Requirements:**
- Multiple shipping options
- Real-time calculation
- Delivery time estimates

---

### TASK 6: Order Tracking (Day 7)

**Requirements:**
- Customer tracking page
- Status updates
- Admin order management

---

## 📦 Database Migrations Needed

### Add Tables:
1. `Coupons` - Store coupon codes and details
2. `CouponUsage` - Track coupon usage per customer
3. `ShippingMethods` - Store shipping options
4. `TaxRates` - Store tax rates by state
5. `OrderTracking` - Store order status updates

### Modify Tables:
1. `Orders` - Add shipping_method_id, tax_amount, estimated_delivery
2. `Users` - Add email_verified, email_verified_at

---

## 🎯 Success Criteria

### Checkout Flow ✅
- [ ] User can review cart items
- [ ] User can select shipping address
- [ ] User can select shipping method
- [ ] Tax and shipping calculated correctly
- [ ] Payment form displays
- [ ] Order created on successful payment
- [ ] Confirmation page shown
- [ ] Email sent to customer

### Coupon System ✅
- [ ] Admin can create coupons
- [ ] Admin can view/edit/delete coupons
- [ ] Frontend shows coupon input
- [ ] Coupon validation works
- [ ] Discount applied to total
- [ ] Error messages show for invalid coupons

### Tax/Shipping ✅
- [ ] Tax calculated by state
- [ ] Shipping costs shown
- [ ] Delivery dates estimated
- [ ] User can change shipping method
- [ ] Costs update in real-time

### Email Notifications ✅
- [ ] Order confirmation email sent
- [ ] Email contains order details
- [ ] Email contains delivery estimate
- [ ] Email contains tracking link

---

## 🚀 Getting Started

**Day 1 Start:**
1. Create checkout DTOs and service
2. Create checkout controller endpoints
3. Set up database migrations
4. Test backend endpoints with Postman

**Day 2 Start:**
1. Create frontend Checkout.tsx page
2. Integrate Stripe Elements
3. Build step-by-step UI
4. Connect to backend

**Day 3 Start:**
1. Create Coupon entity
2. Create CouponService
3. Add API endpoints
4. Test coupon logic

...and so on for remaining tasks

---

## 📊 Estimated Timeline

```
Day 1: Checkout backend         (8 hours)
Day 2: Checkout frontend        (8 hours)
Day 3: Coupon system            (6 hours)
Day 4: Email notifications      (6 hours)
Day 5: Tax calculation          (4 hours)
Day 6: Shipping integration     (6 hours)
Day 7: Order tracking           (6 hours)
_______________________________
Total: 44 hours (5-6 days of work)
```

---

## ✅ Ready to Start!

The next sections will contain the actual implementation code for each component.

