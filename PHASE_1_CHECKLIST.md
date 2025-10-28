# 🚀 PHASE 1 IMPLEMENTATION CHECKLIST - Step-by-Step

**Date:** October 28, 2025  
**Status:** 🟡 IN PROGRESS - Foundation Laid  
**Completion Target:** 7 Days

---

## ✅ What's Been Created So Far

### Database Entities (✅ Done)
- [x] `Coupon.cs` - Coupon and CouponUsage entities
- [x] `ShippingAndTax.cs` - ShippingMethod and TaxRate entities

### Data Transfer Objects (✅ Done)
- [x] `CheckoutDTOs.cs` - All checkout-related DTOs
  - CreateCouponRequest / CouponDTO
  - ValidateCouponRequest / ValidateCouponResponse
  - CheckoutSummaryDTO
  - TaxCalculationDTO / TaxRateDTO
  - ShippingCalculationDTO / ShippingMethodDTO
  - OrderConfirmationDTO

### Service Interfaces (✅ Done)
- [x] `ICouponService` - Coupon management and validation
- [x] `ICheckoutService` - Checkout flow coordination
- [x] `IShippingService` - Shipping method management
- [x] `ITaxService` - Tax rate management

### Repository Interfaces (✅ Done)
- [x] `ICouponRepository` - Coupon data access
- [x] `IShippingMethodRepository` - Shipping method data access
- [x] `ITaxRateRepository` - Tax rate data access

---

## 🔧 What Needs to Be Done

### PHASE 1A: Database Migration (Day 1 - 1 hour)

#### Create EntityFramework Migration
```
Steps:
1. [ ] Add DbSet properties to ApplicationDbContext for: Coupon, CouponUsage, ShippingMethod, TaxRate
2. [ ] Run: dotnet ef migrations add AddCheckoutEntities
3. [ ] Run: dotnet ef database update
4. [ ] Seed initial data (shipping methods, tax rates)
```

**File to Update:** `backend/PetClothingShop.Infrastructure/Data/ApplicationDbContext.cs`

---

### PHASE 1B: Repository Implementation (Day 1 - 2 hours)

#### Create Repository Classes

**File 1:** `backend/PetClothingShop.Infrastructure/Repositories/CouponRepository.cs`
```csharp
// Implement ICouponRepository with:
// - GetByCodeAsync(string code)
// - GetActiveCouponsAsync()
// - GetByCodeWithUsageAsync(string code, int userId)
// - RecordCouponUsageAsync(CouponUsage usage)
// - GetCouponUsageCountAsync(int couponId)
// - GetCouponUsageCountByUserAsync(int couponId, int userId)
// Inherit from Repository<Coupon>
```

**File 2:** `backend/PetClothingShop.Infrastructure/Repositories/ShippingMethodRepository.cs`
```csharp
// Implement IShippingMethodRepository with:
// - GetActiveShippingMethodsAsync()
// - GetByNameAsync(string name)
// Inherit from Repository<ShippingMethod>
```

**File 3:** `backend/PetClothingShop.Infrastructure/Repositories/TaxRateRepository.cs`
```csharp
// Implement ITaxRateRepository with:
// - GetByStateCodeAsync(string stateCode)
// - GetActiveRatesAsync()
// Inherit from Repository<TaxRate>
```

---

### PHASE 1C: Service Implementation (Day 2-3 - 4 hours)

#### Create Service Classes

**File 1:** `backend/PetClothingShop.Infrastructure/Services/CouponService.cs`
```csharp
// Implement ICouponService with:
// - CreateCouponAsync(CreateCouponRequest request)
// - GetCouponByCodeAsync(string code)
// - GetCouponByIdAsync(int id)
// - GetAllCouponsAsync()
// - GetActiveCouponsAsync()
// - UpdateCouponAsync(int id, CreateCouponRequest request)
// - DeleteCouponAsync(int id)
// - ValidateCouponAsync(ValidateCouponRequest request)
// - ApplyCouponAsync(int userId, string couponCode, decimal orderSubtotal, List<int> productIds)
// - RecordCouponUsageAsync(int couponId, int userId, int? orderId, decimal discountAmount)

// Validation Logic:
// - Check if coupon code exists
// - Check if coupon is active
// - Check if coupon has not expired
// - Check if coupon has not started
// - Check if order amount meets minimum requirement
// - Check if product/category applies to coupon
// - Check usage limits (global and per-customer)
// - Calculate discount amount
```

**File 2:** `backend/PetClothingShop.Infrastructure/Services/TaxService.cs`
```csharp
// Implement ITaxService with:
// - GetTaxRatesAsync()
// - GetTaxRateByStateAsync(string stateCode)
// - CreateTaxRateAsync(TaxRateDTO request)
// - UpdateTaxRateAsync(int id, TaxRateDTO request)
// - DeleteTaxRateAsync(int id)
// - CalculateTaxAsync(string stateCode, decimal subtotal)
```

**File 3:** `backend/PetClothingShop.Infrastructure/Services/ShippingService.cs`
```csharp
// Implement IShippingService with:
// - GetShippingMethodsAsync()
// - GetShippingMethodByIdAsync(int id)
// - CreateShippingMethodAsync(ShippingMethodDTO request)
// - UpdateShippingMethodAsync(int id, ShippingMethodDTO request)
// - DeleteShippingMethodAsync(int id)
// - CalculateShippingCostAsync(int shippingMethodId, decimal weight, string stateCode)

// Calculation Logic:
// - Get shipping method details
// - Calculate base cost
// - Add weight-based cost if applicable
// - Calculate estimated delivery date
// - Return ShippingCalculationDTO
```

**File 4:** `backend/PetClothingShop.Infrastructure/Services/CheckoutService.cs`
```csharp
// Implement ICheckoutService with:
// - GetCheckoutSummaryAsync(int userId)
// - CreateOrderAsync(int userId, CreateOrderFromCheckoutRequest request)
// - CalculateTaxAsync(string stateCode, decimal subtotal)
// - CalculateShippingAsync(int shippingMethodId, string stateCode, decimal weight = 5m)
// - GetAvailableShippingMethodsAsync()
// - GetTaxRatesAsync()

// Checkout Flow:
// 1. Get user's cart
// 2. Get user's addresses
// 3. Calculate subtotal from cart items
// 4. Get available shipping methods
// 5. Return summary DTO

// Order Creation Flow:
// 1. Validate cart is not empty
// 2. Validate shipping address belongs to user
// 3. Calculate tax based on state
// 4. Calculate shipping cost
// 5. Apply coupon if provided
// 6. Create order with OrderItems
// 7. Clear cart
// 8. Trigger payment service
// 9. Return confirmation DTO
```

---

### PHASE 1D: API Controllers (Day 3 - 2 hours)

#### Create API Endpoints

**File 1:** `backend/PetClothingShop.API/Controllers/CouponsController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    // Endpoints:
    [POST("/validate")] - Validate coupon without applying
    [POST("/apply")] - Apply coupon and get discount
    [GET] - Get all active coupons
    [GET("{id}")] - Get specific coupon
    [POST] [Authorize(Roles="Admin")] - Create coupon
    [PUT("{id}")] [Authorize(Roles="Admin")] - Update coupon
    [DELETE("{id}")] [Authorize(Roles="Admin")] - Delete coupon
}
```

**File 2:** `backend/PetClothingShop.API/Controllers/CheckoutController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly IPaymentService _paymentService;

    // Endpoints:
    [GET] [Authorize] - Get checkout summary
    [POST] [Authorize] - Create order from checkout
    [GET("shipping-methods")] - Get available shipping methods
    [POST("calculate-tax")] - Calculate tax for state
    [POST("calculate-shipping")] - Calculate shipping cost
}
```

**File 3:** `backend/PetClothingShop.API/Controllers/TaxRatesController.cs` (Admin Only)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TaxRatesController : ControllerBase
{
    // Endpoints:
    [GET] - Get all tax rates
    [GET("{id}")] - Get specific tax rate
    [POST] - Create tax rate
    [PUT("{id}")] - Update tax rate
    [DELETE("{id}")] - Delete tax rate
}
```

**File 4:** `backend/PetClothingShop.API/Controllers/ShippingMethodsController.cs` (Admin Only)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ShippingMethodsController : ControllerBase
{
    // Endpoints:
    [GET] - Get all shipping methods
    [GET("{id}")] - Get specific method
    [POST] - Create shipping method
    [PUT("{id}")] - Update shipping method
    [DELETE("{id}")] - Delete shipping method
}
```

---

### PHASE 1E: Frontend - Checkout Page (Day 4-5 - 4 hours)

#### Create Frontend Components

**File 1:** `frontend/src/pages/Checkout.tsx` - Main checkout page
```tsx
// 4-Step Process:
// Step 1: Review Cart
//   - Display cart items
//   - Quantity adjustments
//   - Remove items
//   - Subtotal display

// Step 2: Shipping Address
//   - List user addresses
//   - Select address or add new
//   - Address validation

// Step 3: Shipping & Tax
//   - Select shipping method
//   - Display shipping cost
//   - Display tax amount
//   - Display total

// Step 4: Payment
//   - Display order summary
//   - Coupon input (optional)
//   - Apply coupon button
//   - Stripe payment form
//   - Submit button
//   - Order confirmation

// State Management (Zustand):
// - Current step (1-4)
// - Selected address
// - Selected shipping method
// - Applied coupon
// - Order data
```

**File 2:** `frontend/src/components/CheckoutStep1.tsx` - Cart Review
```tsx
// Display:
// - Product image, name, quantity
// - Price per item
// - Line total
// - Subtotal
// - Navigation buttons (Back, Next)
```

**File 3:** `frontend/src/components/CheckoutStep2.tsx` - Shipping Address
```tsx
// Display:
// - List of user addresses
// - Select button for each
// - Add new address button (modal)
// - Selected address highlight
// - Continue button
```

**File 4:** `frontend/src/components/CheckoutStep3.tsx` - Shipping & Tax
```tsx
// Display:
// - Available shipping methods
// - Shipping method selection
// - Shipping cost
// - Tax display
// - Subtotal
// - Total
// - Continue button
```

**File 5:** `frontend/src/components/CheckoutStep4.tsx` - Payment
```tsx
// Display:
// - Order summary
// - Coupon section:
//   - Input field for coupon code
//   - Validate button
//   - Applied coupon display
//   - Discount amount
// - Stripe payment form (using @stripe/react-stripe-js)
// - Place order button
// - Error handling
```

**File 6:** `frontend/src/components/CouponInput.tsx` - Coupon component
```tsx
// Props:
// - onApply(couponCode: string)
// - onError(message: string)
// - disabled: boolean

// Features:
// - Input field for coupon code
// - Validate button
// - Loading state
// - Error/success messages
// - Apply discount
```

**File 7:** `frontend/src/store/checkoutStore.ts` - State management
```ts
// State:
// - currentStep: number
// - selectedAddressId: number | null
// - selectedShippingMethodId: number | null
// - appliedCoupon: CouponDTO | null
// - discountAmount: number
// - taxAmount: number
// - shippingCost: number
// - isLoading: boolean
// - error: string | null

// Actions:
// - setCurrentStep(step: number)
// - setSelectedAddress(id: number)
// - setSelectedShippingMethod(id: number)
// - applyCoupon(couponCode: string)
// - removeCoupon()
// - calculateTotals()
// - submitOrder()
// - reset()
```

---

### PHASE 1F: Frontend - Order Confirmation (Day 5 - 1 hour)

#### Create Order Confirmation Page

**File:** `frontend/src/pages/OrderConfirmation.tsx`
```tsx
// Display:
// - Order number
// - Confirmation message
// - Order items
// - Shipping address
// - Delivery estimate
// - Total amount
// - Email confirmation notice
// - Continue shopping button
// - View order details button

// Navigation:
// - Redirect to this page after successful payment
// - Show order ID in URL params
```

---

### PHASE 1G: Email Service Integration (Day 6 - 2 hours)

#### Install SendGrid NuGet Package

**Command:**
```powershell
cd backend/PetClothingShop.API
dotnet add package SendGrid
```

#### Create Email Service

**File 1:** `backend/PetClothingShop.Infrastructure/Services/EmailService.cs`
```csharp
public interface IEmailService
{
    Task SendOrderConfirmationEmailAsync(OrderDTO order, string recipientEmail);
    Task SendShipmentNotificationEmailAsync(int orderId, string trackingNumber, string recipientEmail);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
    Task SendNewsletterAsync(string email, string subject, string content);
}

public class EmailService : IEmailService
{
    // Implementation with SendGrid API
    // Templates for:
    // - Order confirmation with items, total, delivery date
    // - Shipment notification with tracking link
    // - Password reset with reset link
    // - Newsletter content
}
```

**File 2:** `backend/PetClothingShop.API/appsettings.json` - Update with SendGrid key
```json
{
  "SendGrid": {
    "ApiKey": "your-sendgrid-api-key",
    "FromEmail": "noreply@petclothingshop.com",
    "FromName": "Pet Clothing Shop"
  }
}
```

**File 3:** Update `Program.cs` to register service
```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
```

#### Trigger Email on Order Creation

**Update:** `CheckoutService.CreateOrderAsync()`
```csharp
// After order is created successfully:
await _emailService.SendOrderConfirmationEmailAsync(orderDTO, user.Email);
```

---

### PHASE 1H: Testing (Day 7 - 2 hours)

#### Create Unit Tests

**File 1:** `backend/Tests/CouponServiceTests.cs`
```csharp
// Tests:
// - ValidateCouponAsync() - valid coupon
// - ValidateCouponAsync() - expired coupon
// - ValidateCouponAsync() - insufficient order amount
// - ApplyCouponAsync() - correct discount calculation
// - RecordCouponUsageAsync() - usage tracking
```

**File 2:** `backend/Tests/CheckoutServiceTests.cs`
```csharp
// Tests:
// - GetCheckoutSummaryAsync() - returns correct data
// - CreateOrderAsync() - order creation
// - CalculateTaxAsync() - tax calculation by state
// - CalculateShippingAsync() - shipping cost calculation
```

#### Create Integration Tests

**File:** `backend/Tests/CheckoutApiTests.cs`
```csharp
// Tests:
// - POST /api/checkout - create order
// - GET /api/checkout - get summary
// - POST /api/coupons/validate - validate coupon
// - GET /api/shipping-methods - get methods
```

---

## 📝 Database Schema Changes

### New Tables:

#### Coupons Table
```sql
CREATE TABLE "Coupons" (
    "Id" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) UNIQUE NOT NULL,
    "Description" TEXT,
    "DiscountPercentage" DECIMAL(5,2),
    "FixedDiscountAmount" DECIMAL(10,2),
    "MaxDiscountAmount" DECIMAL(10,2),
    "MinimumOrderAmount" DECIMAL(10,2),
    "MaxUsageCount" INT,
    "MaxUsagePerCustomer" INT,
    "StartDate" TIMESTAMP,
    "ExpiryDate" TIMESTAMP,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "ApplicableCategories" TEXT,
    "ApplicableProducts" TEXT,
    "CouponType" VARCHAR(20),
    "CurrentUsageCount" INT DEFAULT 0,
    "CreatedAt" TIMESTAMP DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);

CREATE TABLE "CouponUsages" (
    "Id" SERIAL PRIMARY KEY,
    "CouponId" INT NOT NULL REFERENCES "Coupons"("Id") ON DELETE CASCADE,
    "UserId" INT NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "OrderId" INT REFERENCES "Orders"("Id") ON DELETE SET NULL,
    "DiscountAmount" DECIMAL(10,2),
    "UsedAt" TIMESTAMP DEFAULT NOW()
);

CREATE TABLE "ShippingMethods" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT,
    "BaseCost" DECIMAL(10,2),
    "CostPerWeight" DECIMAL(10,2),
    "MinDeliveryDays" INT,
    "MaxDeliveryDays" INT,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "DisplayOrder" INT,
    "SupportedRegions" TEXT,
    "MaxWeight" DECIMAL(10,2),
    "CarrierName" VARCHAR(100),
    "TrackingUrlPattern" VARCHAR(255),
    "IntegrationProvider" VARCHAR(50),
    "CreatedAt" TIMESTAMP DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);

CREATE TABLE "TaxRates" (
    "Id" SERIAL PRIMARY KEY,
    "StateCode" VARCHAR(5) NOT NULL,
    "StateName" VARCHAR(100),
    "TaxPercentage" DECIMAL(5,2),
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMP DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);
```

### Modify Orders Table:
```sql
ALTER TABLE "Orders" ADD COLUMN "ShippingMethodId" INT REFERENCES "ShippingMethods"("Id");
ALTER TABLE "Orders" ADD COLUMN "TaxAmount" DECIMAL(10,2) DEFAULT 0;
ALTER TABLE "Orders" ADD COLUMN "EstimatedDelivery" TIMESTAMP;
```

---

## 🎯 Implementation Priority & Timeline

```
Day 1:   Database Migration + Repositories    (3 hours)
Day 2:   Tax, Shipping, Coupon Services       (4 hours)
Day 3:   Checkout Service + Controllers       (3 hours)
Day 4-5: Frontend Checkout UI                 (4 hours)
Day 6:   Email Integration                    (2 hours)
Day 7:   Testing + Refinement                 (2 hours)
         ========================================
         Total: 18 hours of focused work
```

---

## ✅ Success Criteria

- [x] Backend compiles without errors
- [ ] All services working with unit tests passing
- [ ] API endpoints responding correctly
- [ ] Frontend checkout page fully functional
- [ ] Payment flow working end-to-end
- [ ] Emails sending successfully
- [ ] Tax calculated correctly
- [ ] Shipping options working
- [ ] Coupons applying correctly
- [ ] Order confirmation page showing
- [ ] Database migrations successful

---

## 🚀 Next Document: Detailed Implementation Code

The next document will contain the complete implementation code for each component, ready to copy-paste into your project.

