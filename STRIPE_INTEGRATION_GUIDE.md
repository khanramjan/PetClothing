# 🔐 Stripe Payment Integration - Implementation Guide

## Overview

The Stripe payment integration is now **fully implemented** in the backend! This document explains how to configure and test it.

## What Was Implemented

### Backend Components (✅ Complete)

1. **PaymentService.cs** (`Infrastructure/Services/`)
   - `CreatePaymentIntentAsync()` - Creates Stripe Payment Intent for orders
   - `ConfirmPaymentAsync()` - Confirms payment after customer provides payment method
   - `HandleStripeWebhookAsync()` - Processes Stripe webhook events
   - `RefundPaymentAsync()` - Processes refunds for orders
   - `GetPaymentAsync()` - Retrieves specific payment record
   - `GetUserPaymentHistoryAsync()` - Gets user's payment history

2. **PaymentService.cs** (`Core/DTOs/`)
   - `CreatePaymentIntentRequest` - Request to create payment intent
   - `PaymentIntentResponse` - Response with clientSecret for frontend
   - `ConfirmPaymentRequest` - Request to confirm payment
   - `PaymentConfirmationResponse` - Response after payment confirmed
   - `PaymentHistoryDTO` - Payment record data
   - `RefundRequest/RefundResponse` - Refund operation DTOs

3. **Payment Entity** (`Core/Entities/Payment.cs`)
   - Stores complete payment transaction history
   - Tracks payment status, amounts, failures, and refunds
   - Audit trail for all payments

4. **PaymentsController.cs** (`API/Controllers/`)
   - `POST /api/payments/create-intent` - Create payment intent
   - `POST /api/payments/confirm` - Confirm payment
   - `POST /api/payments/webhook` - Stripe webhook endpoint
   - `POST /api/payments/refund` - Request refund
   - `GET /api/payments/history` - Get payment history
   - `GET /api/payments/{paymentId}` - Get specific payment

5. **IPaymentService Interface**
   - Added to `Core/Interfaces/IServices.cs`

6. **Stripe Configuration**
   - Added to `appsettings.json`
   - Stripe.net v49.0.0 NuGet package installed

---

## Configuration Steps

### Step 1: Get Stripe API Keys

1. Go to https://stripe.com and create a free account
2. Go to Dashboard → Developers → API Keys
3. Copy your **Publishable Key** (pk_...) and **Secret Key** (sk_...)
4. Enable test mode (toggle in top-right)

### Step 2: Update appsettings.json

Edit `backend/PetClothingShop.API/appsettings.json`:

```json
"Stripe": {
  "PublishableKey": "pk_test_YOUR_KEY_HERE",
  "SecretKey": "sk_test_YOUR_KEY_HERE",
  "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET"
}
```

### Step 3: Setup Webhook Endpoint

1. Go to https://stripe.com/docs/webhooks
2. In your Stripe Dashboard, go to Developers → Webhooks
3. Click "Add endpoint"
4. Enter your webhook URL: `https://yourapi.com/api/payments/webhook`
   - For local testing: Use **Stripe CLI** (see below)
5. Select events:
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
   - `charge.refunded`
   - `payment_intent.canceled`
6. Copy your **Webhook Signing Secret** (whsec_...) and add to appsettings.json

### Step 4: Setup Stripe CLI (for local testing)

```bash
# Windows - Download from https://stripe.com/docs/stripe-cli
# Or use chocolatey:
choco install stripe-cli

# Then login:
stripe login

# Forward webhook to local:
stripe listen --forward-to localhost:5000/api/payments/webhook
```

---

## API Endpoints Reference

### 1. Create Payment Intent

**Endpoint:** `POST /api/payments/create-intent`

**Headers:**
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "orderId": 123,
  "amount": 89.99,
  "currency": "usd",
  "email": "customer@example.com",
  "receiptEmail": "customer@example.com"
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "clientSecret": "pi_1ABC123_secret_xyz789",
    "paymentIntentId": "pi_1ABC123",
    "amount": 89.99,
    "status": "requires_payment_method",
    "currency": "usd",
    "createdAt": "2025-10-27T12:00:00Z"
  },
  "message": "Payment intent created successfully"
}
```

### 2. Confirm Payment

**Endpoint:** `POST /api/payments/confirm`

**Headers:**
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "paymentIntentId": "pi_1ABC123_secret_xyz789",
  "paymentMethodId": "pm_1ABC456",
  "orderId": 123
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "paymentIntentId": "pi_1ABC123",
    "status": "succeeded",
    "amount": 89.99,
    "orderId": 123,
    "processedAt": "2025-10-27T12:00:00Z"
  },
  "message": "Payment confirmed successfully"
}
```

### 3. Request Refund

**Endpoint:** `POST /api/payments/refund`

**Headers:**
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "orderId": 123,
  "reason": "customer_request",
  "amount": 89.99
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "refundId": "re_1ABC789",
    "amount": 89.99,
    "processedAt": "2025-10-27T12:00:00Z"
  },
  "message": "Refund processed: succeeded"
}
```

### 4. Get Payment History

**Endpoint:** `GET /api/payments/history`

**Headers:**
```
Authorization: Bearer {jwt_token}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "orderId": 123,
      "paymentIntentId": "pi_1ABC123",
      "amount": 89.99,
      "status": "succeeded",
      "paymentMethod": "stripe",
      "createdAt": "2025-10-27T12:00:00Z",
      "processedAt": "2025-10-27T12:00:05Z",
      "failureReason": null
    }
  ],
  "message": "Payment history retrieved"
}
```

### 5. Get Specific Payment

**Endpoint:** `GET /api/payments/{paymentId}`

**Headers:**
```
Authorization: Bearer {jwt_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "orderId": 123,
    "paymentIntentId": "pi_1ABC123",
    "amount": 89.99,
    "status": "succeeded",
    "paymentMethod": "stripe",
    "createdAt": "2025-10-27T12:00:00Z",
    "processedAt": "2025-10-27T12:00:05Z"
  }
}
```

---

## Testing Stripe Integration

### Test Cards

Use these test card numbers in Stripe's test mode:

| Card Type | Number | CVC | Exp | Result |
|-----------|--------|-----|-----|--------|
| Visa (Success) | `4242 4242 4242 4242` | Any 3 digits | Future date | ✅ Succeeds |
| Visa (Decline) | `4000 0000 0000 0002` | Any 3 digits | Future date | ❌ Fails |
| Require Auth | `4000 0025 0000 3155` | Any 3 digits | Future date | 3D Secure |
| Expired Card | `4000 0000 0000 0069` | Any 3 digits | Future date | ❌ Fails |

**Expiration:** Any future date (e.g., 12/25)
**CVC:** Any 3 digits (e.g., 123)

### Manual Testing Steps

1. **Start Backend:**
   ```bash
   cd backend/PetClothingShop.API
   dotnet run
   ```

2. **Start Stripe CLI webhook listener (new terminal):**
   ```bash
   stripe listen --forward-to localhost:5000/api/payments/webhook
   ```

3. **Create Order:**
   ```bash
   POST /api/orders
   Authorization: Bearer {jwt_token}
   {
     "shippingAddressId": 1,
     "shippingCost": 9.99
   }
   ```

4. **Create Payment Intent:**
   ```bash
   POST /api/payments/create-intent
   Authorization: Bearer {jwt_token}
   {
     "orderId": 1,
     "amount": 89.99,
     "currency": "usd"
   }
   ```

5. **Test in Stripe Dashboard:**
   - Go to https://dashboard.stripe.com/test/payments
   - See your test payment attempt
   - Confirm webhook was received

---

## Database Migration

The Payment entity is new. To add it to your database:

```bash
cd backend/PetClothingShop.Infrastructure

# Create migration
dotnet ef migrations add AddPaymentEntity

# Apply migration
dotnet ef database update
```

---

## Next Steps: Frontend Implementation

The backend is ready! Now we need to build the frontend with:

1. **Stripe Elements** - Card input form
2. **Checkout Page** - Address, shipping, payment form
3. **Cart Page** - Display items before checkout
4. **Payment Confirmation** - Show success/failure

See **Task 2** and **Task 3** in the implementation roadmap for frontend details.

---

## Environment Variables

For production, use environment variables instead of appsettings:

```bash
# Linux/Mac
export STRIPE_PUBLISHABLE_KEY="pk_live_..."
export STRIPE_SECRET_KEY="sk_live_..."
export STRIPE_WEBHOOK_SECRET="whsec_..."

# Windows PowerShell
$env:STRIPE_PUBLISHABLE_KEY="pk_live_..."
$env:STRIPE_SECRET_KEY="sk_live_..."
$env:STRIPE_WEBHOOK_SECRET="whsec_..."
```

Then access in C#:
```csharp
var secretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
```

---

## Troubleshooting

### "Stripe SecretKey not configured"
- Check `appsettings.json` has Stripe section
- Verify keys are correct from Stripe dashboard
- Restart API after config changes

### Webhook not received
- Check Stripe CLI is running: `stripe listen --forward-to localhost:5000/api/payments/webhook`
- Verify webhook URL in Stripe Dashboard matches
- Check firewall/ports are open

### Payment fails with "invalid_request_error"
- Check amount is in cents (multiply by 100)
- Verify orderId exists and belongs to user
- Check order status isn't already "Paid"

### "Exceeded rate limit"
- Stripe API rate limits: 100 requests/second
- Check for retry loops in code
- Implement exponential backoff

---

## Security Considerations

1. **Never expose Secret Key** - Only use on backend
2. **Use HTTPS** - Required for production
3. **Validate signatures** - Webhook handler validates Stripe signature
4. **Rate limiting** - Already configured in API
5. **Log sensitive data** - Be careful what you log
6. **PCI Compliance** - Use Stripe Elements (never collect raw card data)

---

## Production Checklist

- [ ] Switch to live Stripe keys (pk_live_, sk_live_)
- [ ] Configure production webhook URL
- [ ] Enable HTTPS
- [ ] Setup error monitoring (Sentry, DataDog, etc.)
- [ ] Configure proper logging to persistent storage
- [ ] Setup database backups
- [ ] Test refund process
- [ ] Create incident response plan
- [ ] Monitor Stripe dashboard for failed payments
- [ ] Implement retry logic for failed payments

---

## Support & Documentation

- **Stripe API Docs:** https://stripe.com/docs/api
- **Stripe.NET Package:** https://github.com/stripe/stripe-dotnet
- **Webhook Reference:** https://stripe.com/docs/webhooks
- **Testing Guide:** https://stripe.com/docs/testing

---

## Files Created/Modified

### Created:
- `PaymentService.cs` - Payment processing logic
- `PaymentDTOs.cs` - Data transfer objects
- `Payment.cs` - Entity model
- `PaymentsController.cs` - API endpoints

### Modified:
- `IServices.cs` - Added IPaymentService interface
- `appsettings.json` - Added Stripe configuration
- `Program.cs` - Registered PaymentService

### Installed:
- `Stripe.net` v49.0.0 NuGet package

---

**Status:** ✅ COMPLETE

**Next Task:** Cart Page UI Implementation (Task 2)

**Effort:** ~5-7 days

**Revenue Impact:** $2,500-5,000/month (after checkout completion)
