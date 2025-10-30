# SSLCommerz 404 Error - Root Cause & Fix

## Problem
You were receiving a **404 (Not Found)** error when trying to access the SSLCommerz cashier page:

```
GET https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=khani68f514d22504a%7CORDER-1761717601694-20251029060001%7C152.99%7CBDT%7C0 
→ 404 (Not Found)
```

## Root Cause
The original `InitiateSSLCommerz` controller endpoint was **manually building a sessionkey string** instead of calling the SSLCommerz API to obtain a valid one:

```csharp
// ❌ WRONG - Manual sessionkey construction
var sessionKey = $"{storeId}|{transactionId}|{request.Amount}|{request.Currency}|{userId}";
var gatewayUrl = $"{cashierUrl}?sessionkey={Uri.EscapeDataString(sessionKey)}";
```

**SSLCommerz does NOT recognize manually constructed sessionkeys.** The cashier endpoint expects a valid sessionkey obtained from SSLCommerz's API.

## How SSLCommerz Payment Flow Works

```
1. Your Backend → POST /gwprocess/v4/api.php (with credentials + payment details)
                  ↓
2. SSLCommerz API → Returns valid sessionkey
                  ↓
3. Your Backend → Pass sessionkey to frontend
                  ↓
4. Frontend → Redirect user to https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=VALID_KEY
                  ↓
5. SSLCommerz Cashier → ✅ Accepts the valid sessionkey
```

## The Fix

### Changes Made

#### 1. Added SSLCommerz API DTOs (`PaymentDTOs.cs`)
```csharp
public class SSLCommerzSessionRequest
{
    public string store_id { get; set; }
    public string store_passwd { get; set; }
    public string total_amount { get; set; }
    // ... other payment details
}

public class SSLCommerzSessionResponse
{
    public string status { get; set; }
    public string sessionkey { get; set; }  // ← The key you need!
    public string gateway_url { get; set; }
}
```

#### 2. Added `InitiateSSLCommerzPaymentAsync()` to PaymentService
This method:
- Calls SSLCommerz API endpoint: `POST /gwprocess/v4/api.php`
- Sends store credentials and payment details
- **Receives a valid sessionkey from SSLCommerz**
- Returns the cashier URL with the proper sessionkey

```csharp
public async Task<InitiatePaymentResponse> InitiateSSLCommerzPaymentAsync(InitiatePaymentRequest request)
{
    // Call SSLCommerz API to create session
    using (var httpClient = new HttpClient())
    {
        var content = new FormUrlEncodedContent(ConvertToKeyValuePairs(sslRequest));
        var response = await httpClient.PostAsync(apiUrl, content);
        
        // Parse response to get sessionkey
        var sslResponse = JsonSerializer.Deserialize<SSLCommerzSessionResponse>(responseContent);
        
        // Build cashier URL with VALID sessionkey
        var gatewayUrl = $"{cashierUrl}?sessionkey={Uri.EscapeDataString(sslResponse.sessionkey)}";
    }
}
```

#### 3. Updated PaymentsController
The `/api/payments/initiate` endpoint now:
- Calls the service method instead of building the URL manually
- Validates required customer information
- Returns the properly generated gateway URL

```csharp
[HttpPost("initiate")]
[AllowAnonymous]
public async Task<IActionResult> InitiateSSLCommerz([FromBody] InitiatePaymentRequest request)
{
    var response = await _paymentService.InitiateSSLCommerzPaymentAsync(request);
    return Ok(new { success = true, data = response });
}
```

## How to Test

### 1. Make a POST request to your endpoint:
```bash
POST http://localhost:5000/api/payments/initiate
Content-Type: application/json

{
  "amount": 152.99,
  "currency": "BDT",
  "orderId": "12345",
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "customerPhone": "+8801234567890",
  "description": "Pet Clothing Purchase",
  "successUrl": "https://yoursite.com/payment-success",
  "failUrl": "https://yoursite.com/payment-failed",
  "cancelUrl": "https://yoursite.com/payment-cancelled"
}
```

### 2. Response will be:
```json
{
  "success": true,
  "data": {
    "transactionId": "ORDER-20251029060001-ABCD1234",
    "gatewayPageURL": "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=VALID_SESSION_KEY_FROM_API",
    "message": "Payment gateway URL generated successfully"
  },
  "message": "Payment gateway URL generated successfully"
}
```

### 3. Redirect to `gatewayPageURL`:
The URL now has a **valid sessionkey** returned from SSLCommerz API, so the cashier page will load ✅

## Configuration Requirements

Ensure your `appsettings.json` has:
```json
{
  "SSLCommerz": {
    "StoreId": "your-store-id",
    "StorePassword": "your-store-password",
    "ApiUrl": "https://sandbox.sslcommerz.com/gwprocess/v4/api.php",
    "CashierUrl": "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php",
    "IsProduction": false
  }
}
```

Or set environment variables:
```
SSLCOMMERZ_STORE_ID=your-store-id
SSLCOMMERZ_STORE_PASSWORD=your-store-password
```

## API Endpoints Updated

### POST `/api/payments/initiate`
- **Before**: Manually built invalid sessionkey → 404 error
- **After**: Calls SSLCommerz API to get valid sessionkey → ✅ Works

## Summary of Files Modified

1. **`PetClothingShop.Core/DTOs/PaymentDTOs.cs`**
   - Added `SSLCommerzSessionRequest` DTO
   - Added `SSLCommerzSessionResponse` DTO

2. **`PetClothingShop.Core/Interfaces/IServices.cs`**
   - Added `InitiateSSLCommerzPaymentAsync()` method to `IPaymentService` interface

3. **`PetClothingShop.Infrastructure/Services/PaymentService.cs`**
   - Added `InitiateSSLCommerzPaymentAsync()` implementation
   - Added `ConvertToKeyValuePairs()` helper method
   - Added `System.Net.Http.Json` and `System.Text.Json` imports

4. **`PetClothingShop.API/Controllers/PaymentsController.cs`**
   - Updated `InitiateSSLCommerz()` to call service method
   - Added validation for required customer information

## Next Steps

1. **Test the payment flow** with a SSLCommerz test merchant account
2. **Monitor logs** to ensure the API communication works
3. **Update frontend** to properly redirect to the `gatewayPageURL` returned
4. **Implement callback handling** for payment success/failure in another endpoint
5. **Deploy to production** and update to real credentials when ready

## References

- SSLCommerz Documentation: https://www.sslcommerz.com/download/SSLCommerz_Integration_Guide.pdf
- SSLCommerz Sandbox: https://sandbox.sslcommerz.com/
