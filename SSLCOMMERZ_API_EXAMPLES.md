# SSLCommerz - API Request & Response Examples

## Your Backend to SSLCommerz API Communication

### Step 1: Backend Receives Request from Frontend
**Endpoint:** `POST /api/payments/initiate`

**Request Body:**
```json
{
  "amount": 152.99,
  "currency": "BDT",
  "orderId": "12345",
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "customerPhone": "+8801234567890",
  "description": "Pet Clothing Purchase",
  "successUrl": "https://yoursite.com/payment/success",
  "failUrl": "https://yoursite.com/payment/failed",
  "cancelUrl": "https://yoursite.com/payment/cancelled"
}
```

**Response (from your backend):**
```json
{
  "success": true,
  "data": {
    "transactionId": "ORDER-20251029060001-ABCD1234",
    "gatewayPageURL": "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=khani68f514d22504a%7CORDER-20251029-ABCD1234%7C152.99%7CBDT%7C0",
    "message": "Payment gateway URL generated successfully"
  },
  "message": "Payment gateway URL generated successfully"
}
```

---

### Step 2: Backend to SSLCommerz API Communication

**What Your Backend Sends to SSLCommerz:**

#### Request Type
```
POST https://sandbox.sslcommerz.com/gwprocess/v4/api.php
Content-Type: application/x-www-form-urlencoded
```

#### Request Body (Form Data)
```
store_id=testbox
store_passwd=qwerty
total_amount=152.99
currency=BDT
tran_id=ORDER-20251029060001-ABCD1234
success_url=https://yoursite.com/payment/success
fail_url=https://yoursite.com/payment/failed
cancel_url=https://yoursite.com/payment/cancelled
cus_name=John+Doe
cus_email=john%40example.com
cus_phone=%2B8801234567890
product_name=Pet+Clothing+Purchase
product_category=clothing
product_profile=general
```

#### What SSLCommerz Returns

**Response Type:** `application/json` or `application/x-www-form-urlencoded`

**If Successful (JSON):**
```json
{
  "status": "SUCCESS",
  "sessionkey": "khani68f514d22504a|ORDER-20251029060001-ABCD1234|152.99|BDT|0",
  "gateway_url": "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=khani68f514d22504a%7CORDER-20251029060001-ABCD1234%7C152.99%7CBDT%7C0"
}
```

**If Successful (Form Encoded):**
```
status=SUCCESS&sessionkey=khani68f514d22504a%7CORDER-20251029060001-ABCD1234%7C152.99%7CBDT%7C0&gateway_url=https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=khani68f514d22504a%7CORDER-20251029060001-ABCD1234%7C152.99%7CBDT%7C0
```

**If Failed:**
```json
{
  "status": "FAILED",
  "errorMessage": "Invalid Store ID"
}
```

---

## Complete Request/Response Flow

### Full Example: User Pays for Order

```
CLIENT (Browser)                           BACKEND                                    SSLCOMMERZ
     |                                        |                                          |
     |--- 1. Click "Pay" ----------------→  |                                          |
     |                                        |                                          |
     |                          2. POST /api/payments/initiate                          |
     |                          (amount, customer info, etc)                            |
     |                                        |                                          |
     |                                        |--- 3. POST /gwprocess/v4/api.php ----→ |
     |                                        |    (store_id, store_passwd,             |
     |                                        |     total_amount, customer data, etc)  |
     |                                        |                                          |
     |                                        |                                      [Process]
     |                                        |                                          |
     |                                        | ← 4. Response with sessionkey --------- |
     |                                        |    {status: "SUCCESS",                  |
     |                                        |     sessionkey: "VALID_KEY",            |
     |                                        |     gateway_url: "...cashier.php..."}   |
     |                                        |                                          |
     |                   5. Response ← -------|                                          |
     |  {gatewayPageURL: "...cashier.php..."}|                                          |
     |                                        |                                          |
     |--- 6. Redirect to gatewayPageURL --→ https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=VALID_KEY
     |                                        |                                          |
     |                                        |                                      [Load Payment Form]
     |                                        |                                          |
     |                                     ✅ Payment page loads (NOT 404!)              |
```

---

## cURL Examples for Testing

### Test 1: Valid Payment Request
```bash
curl -X POST http://localhost:5000/api/payments/initiate \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 152.99,
    "currency": "BDT",
    "orderId": "12345",
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "customerPhone": "+8801234567890",
    "description": "Pet Clothing",
    "successUrl": "https://yoursite.com/success",
    "failUrl": "https://yoursite.com/failed",
    "cancelUrl": "https://yoursite.com/cancelled"
  }'
```

**Expected Output:**
```json
{
  "success": true,
  "data": {
    "transactionId": "ORDER-20251029...",
    "gatewayPageURL": "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php?sessionkey=...",
    "message": "Payment gateway URL generated successfully"
  }
}
```

### Test 2: Missing Customer Email (Should Fail)
```bash
curl -X POST http://localhost:5000/api/payments/initiate \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 152.99,
    "currency": "BDT",
    "orderId": "12345",
    "customerName": "John Doe",
    "customerPhone": "+8801234567890"
  }'
```

**Expected Output:**
```json
{
  "success": false,
  "message": "Customer name and email are required"
}
```

### Test 3: Invalid Amount (Should Fail)
```bash
curl -X POST http://localhost:5000/api/payments/initiate \
  -H "Content-Type: application/json" \
  -d '{
    "amount": -50,
    "currency": "BDT",
    "orderId": "12345",
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "customerPhone": "+8801234567890"
  }'
```

**Expected Output:**
```json
{
  "success": false,
  "message": "Invalid amount"
}
```

---

## JavaScript/Fetch Examples

### Make Payment Request
```javascript
async function initiatePayment(orderData) {
  try {
    const response = await fetch('http://localhost:5000/api/payments/initiate', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        amount: orderData.totalAmount,
        currency: 'BDT',
        orderId: orderData.orderId.toString(),
        customerName: orderData.customerName,
        customerEmail: orderData.customerEmail,
        customerPhone: orderData.customerPhone,
        description: 'Pet Clothing Purchase',
        successUrl: `${window.location.origin}/payment/success`,
        failUrl: `${window.location.origin}/payment/failed`,
        cancelUrl: `${window.location.origin}/payment/cancelled`
      })
    });

    const data = await response.json();

    if (data.success) {
      // Redirect to SSLCommerz payment page
      console.log('Gateway URL:', data.data.gatewayPageURL);
      window.location.href = data.data.gatewayPageURL;
    } else {
      console.error('Payment failed:', data.message);
      // Show error to user
    }
  } catch (error) {
    console.error('Network error:', error);
  }
}
```

---

## Postman Collection

### Import into Postman

```json
{
  "info": {
    "name": "SSLCommerz Payment API",
    "description": "Pet Clothing Shop Payment Integration"
  },
  "item": [
    {
      "name": "Initiate Payment",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "url": {
          "raw": "http://localhost:5000/api/payments/initiate",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "payments", "initiate"]
        },
        "body": {
          "mode": "raw",
          "raw": "{\n  \"amount\": 152.99,\n  \"currency\": \"BDT\",\n  \"orderId\": \"12345\",\n  \"customerName\": \"John Doe\",\n  \"customerEmail\": \"john@example.com\",\n  \"customerPhone\": \"+8801234567890\",\n  \"description\": \"Pet Clothing Purchase\",\n  \"successUrl\": \"https://yoursite.com/success\",\n  \"failUrl\": \"https://yoursite.com/failed\",\n  \"cancelUrl\": \"https://yoursite.com/cancelled\"\n}"
        }
      }
    }
  ]
}
```

---

## Important Notes

### Field Mappings

| Frontend Field | Backend DTO | SSLCommerz Field | Description |
|---|---|---|---|
| `amount` | `Amount` | `total_amount` | Payment amount |
| `currency` | `Currency` | `currency` | Currency code (BDT, USD, etc) |
| `orderId` | `OrderId` | `tran_id` | Transaction/Order ID |
| `customerName` | `CustomerName` | `cus_name` | Customer full name |
| `customerEmail` | `CustomerEmail` | `cus_email` | Customer email |
| `customerPhone` | `CustomerPhone` | `cus_phone` | Customer phone |
| `description` | `Description` | `product_name` | Product/Order description |

### Response Headers
- `status` = "SUCCESS" means the API call worked
- `sessionkey` is the VALID key to use with cashier page
- `gateway_url` can be used directly or built manually

### URL Encoding
- Spaces → `+` or `%20`
- Special chars → URL encoded (e.g., `@` → `%40`)
- Sessionkey is pipe-delimited: `store_id|order_id|amount|currency|user_id`

---

## Debugging: Check These Values

1. **Store ID**: `testbox` (for sandbox)
2. **Store Password**: `qwerty` (for sandbox)
3. **API URL**: `https://sandbox.sslcommerz.com/gwprocess/v4/api.php`
4. **Cashier URL**: `https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php`
5. **Amount**: Must be > 0
6. **Transaction ID**: Must be unique per transaction
7. **URLs**: Must be valid and accessible

---

## Verification Steps

1. ✅ Send valid request to `/api/payments/initiate`
2. ✅ Check backend logs for "SSLCommerz API response"
3. ✅ Verify response contains `gatewayPageURL`
4. ✅ Extract `sessionkey` from response
5. ✅ Visit `gatewayPageURL` in browser
6. ✅ Should see SSLCommerz payment page (NOT 404)
7. ✅ Test payment with SSLCommerz test card

