# SSLCommerz Integration - Quick Reference

## The Issue in One Sentence
**You were building a sessionkey string manually instead of requesting it from SSLCommerz's API, which rejects unknown sessionkeys with a 404 error.**

## Visual Comparison

### ❌ What You Were Doing (WRONG)
```
Frontend                  Your Backend              SSLCommerz
   |                           |                         |
   |--- Initiate Payment ----→ |                         |
   |                           |                         |
   |                    [Build fake sessionkey]          |
   |                  "store|order|amount|currency|user" |
   |                           |                         |
   | ← Return cashier URL ---- | SSLCommerz Cashier     |
   |                           |                         |
   |--- Redirect to -------→ https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=FAKE_KEY
   |                           |                         |
   |                           |                    ❌ 404 - Unknown sessionkey!
```

### ✅ What You Should Do (FIXED)
```
Frontend                  Your Backend              SSLCommerz API
   |                           |                         |
   |--- Initiate Payment ----→ |                         |
   |                           |--- POST /api.php ----→ |
   |                           |  (store_id, passwd,    |
   |                           |   total_amount, etc)    |
   |                           |                         |
   |                           | ← [VALID sessionkey]-- |
   |                           |                         |
   | ← Return cashier URL ---- |                         |
   |  (with VALID sessionkey)  |                         |
   |                           |                         |
   |--- Redirect to -------→ https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=VALID_KEY
   |                           |                         |
   |                           |                    ✅ 200 - sessionkey recognized!
```

## Code Changes Summary

### 1. New DTOs for SSLCommerz API Communication
```csharp
// Request sent TO SSLCommerz API
public class SSLCommerzSessionRequest
{
    public string store_id { get; set; }
    public string store_passwd { get; set; }
    public string total_amount { get; set; }
    public string currency { get; set; }
    public string tran_id { get; set; }
    // ... more fields
}

// Response FROM SSLCommerz API
public class SSLCommerzSessionResponse
{
    public string status { get; set; }           // "SUCCESS" or error
    public string sessionkey { get; set; }       // ← THE VALID KEY!
    public string gateway_url { get; set; }      // or build it yourself
}
```

### 2. Service Method to Call SSLCommerz API
```csharp
public async Task<InitiatePaymentResponse> InitiateSSLCommerzPaymentAsync(InitiatePaymentRequest request)
{
    // 1. Prepare request with store credentials
    var sslRequest = new SSLCommerzSessionRequest
    {
        store_id = storeId,
        store_passwd = storePassword,
        total_amount = request.Amount.ToString("F2"),
        currency = request.Currency,
        tran_id = transactionId,
        cus_name = request.CustomerName,
        cus_email = request.CustomerEmail,
        // ... etc
    };
    
    // 2. Call SSLCommerz API
    var response = await httpClient.PostAsync(apiUrl, content);
    
    // 3. Parse response to get sessionkey
    var sslResponse = JsonSerializer.Deserialize<SSLCommerzSessionResponse>(responseContent);
    
    // 4. Build URL with VALID sessionkey
    var gatewayUrl = $"{cashierUrl}?sessionkey={Uri.EscapeDataString(sslResponse.sessionkey)}";
    
    return new InitiatePaymentResponse { GatewayPageURL = gatewayUrl };
}
```

### 3. Controller Uses Service
```csharp
[HttpPost("initiate")]
public async Task<IActionResult> InitiateSSLCommerz([FromBody] InitiatePaymentRequest request)
{
    // ✅ Call service method (which calls SSLCommerz API)
    var response = await _paymentService.InitiateSSLCommerzPaymentAsync(request);
    return Ok(response);
}
```

## Expected API Flow

### Step 1: Your Frontend Calls Your Backend
```
POST /api/payments/initiate
{
  "amount": 152.99,
  "currency": "BDT",
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "customerPhone": "+88012345678",
  "description": "Order #123"
}
```

### Step 2: Your Backend Calls SSLCommerz API
```
POST https://sandbox.sslcommerz.com/gwprocess/v4/api.php

Form Data:
  store_id=khani
  store_passwd=qwerty
  total_amount=152.99
  currency=BDT
  tran_id=ORDER-20251029-ABC123
  cus_name=John Doe
  cus_email=john@example.com
  cus_phone=+88012345678
  success_url=https://yoursite.com/payment/success
  fail_url=https://yoursite.com/payment/failed
  cancel_url=https://yoursite.com/payment/cancelled
  product_name=Pet Clothing
  product_profile=general
```

### Step 3: SSLCommerz Returns Valid SessionKey
```
HTTP 200 OK

{
  "status": "SUCCESS",
  "sessionkey": "khani68f514d22504a|ORDER-20251029-ABC123|152.99|BDT|0",
  "gateway_url": "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=khani68f514d22504a%7CORDER-20251029-ABC123%7C152.99%7CBDT%7C0"
}
```

### Step 4: Your Backend Returns to Frontend
```
HTTP 200 OK

{
  "success": true,
  "data": {
    "transactionId": "ORDER-20251029-ABC123",
    "gatewayPageURL": "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=khani68f514d22504a%7CORDER-20251029-ABC123%7C152.99%7CBDT%7C0",
    "message": "Payment gateway URL generated successfully"
  }
}
```

### Step 5: Frontend Redirects User
```javascript
// In your React frontend
window.location.href = response.data.gatewayPageURL;
// User is now at SSLCommerz checkout with VALID sessionkey ✅
```

## Configuration Checklist

- [ ] `appsettings.json` has SSLCommerz section configured
- [ ] `SSLCOMMERZ_STORE_ID` environment variable set
- [ ] `SSLCOMMERZ_STORE_PASSWORD` environment variable set
- [ ] API URLs point to correct endpoints (sandbox or production)
- [ ] Success/Fail/Cancel URLs are properly configured

## Testing Checklist

- [ ] Test with valid credentials
- [ ] Monitor logs for API responses
- [ ] Verify sessionkey is present in returned URL
- [ ] Visit the gateway URL in browser
- [ ] Confirm SSLCommerz payment page loads (no 404)
- [ ] Test payment flow to completion

## Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| 404 on cashier page | Invalid/missing sessionkey | Ensure SSLCommerz API returns valid sessionkey |
| SSLCommerz API returns error | Invalid credentials | Check store_id and store_passwd |
| No response from API | Wrong URL or network issue | Verify API endpoint and firewall rules |
| Sessionkey appears invalid | Manually constructed | Let SSLCommerz API generate it |

