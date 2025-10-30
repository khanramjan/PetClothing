# SSLCommerz Checkout - Quick Start Testing Guide

## 🚀 System Status

✅ Backend running on `http://localhost:5000`
✅ Frontend ready at `http://localhost:5173`
✅ SSLCommerz endpoint: `POST /api/payments/initiate`
✅ Database connected and migrations applied

## 📋 Testing Steps

### 1. Add Items to Cart
```
1. Go to Home page
2. Click "Add to Cart" on any product
3. Verify item appears in cart icon badge
```

### 2. View Cart
```
1. Click cart icon in navbar
2. Verify all items show with prices
3. Check subtotal, tax (10%), shipping calculations
4. Click "Proceed to Checkout"
```

### 3. Step 1: Shipping Address
```
Fill in the form:
- First Name: John
- Last Name: Doe
- Email: john@example.com
- Phone: +8801234567890
- Address: 123 Main Street
- City: Dhaka
- State: Dhaka
- Zip Code: 1212

Click "Next"
```

### 4. Step 2: Shipping Method
```
Select one:
- Standard (5-7 days) - $9.99
- Express (2-3 days) - $24.99
- Overnight - $49.99

Verify total updates in sidebar
Click "Next"
```

### 5. Step 3: Order Review
```
Verify:
- Shipping address is correct
- Shipping method selected
- All items listed with prices

Click "Next"
```

### 6. Step 4: Payment
```
Information displayed:
- Payment Gateway: SSLCommerz Sandbox
- Test card details shown
- "Pay Now" button active

Click "Pay Now"
```

### 7. SSLCommerz Gateway
```
Backend sends request to: POST /api/payments/initiate

Response includes:
- Transaction ID: ORDER-{timestamp}
- Gateway URL for payment
- Frontend redirects to gateway
```

### 8. Test Card Payment (Sandbox)
```
On SSLCommerz gateway:
- Card Number: 4111111111111111
- Expiry: 12/25 (or any future)
- CVV: 123

Click "Pay"
```

### 9. Payment Success
```
Redirected to: /checkout/success

Success page shows:
- ✅ Green checkmark
- "Payment Successful!" message
- "Order confirmation email sent" notice
- Cart cleared automatically
- Options to continue shopping or view orders
- Auto-redirects to home after 5 seconds
```

### 10. Payment Failure (Optional Test)
```
Go back to: /checkout
Click "Pay Now" again
Use invalid card: 4111111111111112

Redirected to: /checkout/failed

Failure page shows:
- ❌ Red alert
- "Payment Failed" message
- Cart items still saved
- Options to retry or continue shopping
```

## 🔧 Backend Configuration

### Current Setup (Sandbox Mode):
```
- SSLCommerz Gateway: https://sandbox.sslcommerz.com
- Store Credentials: TO BE ADDED
- Mode: SANDBOX (test mode)
```

### To Use Real Credentials:
Edit `appsettings.json`:
```json
{
  "SSLCommerz": {
    "StoreId": "your_store_id",
    "StorePassword": "your_store_password",
    "IsProduction": false
  }
}
```

## 📱 UI Flow Verification

### Checkout Page Elements to Check:
```
✅ Progress indicator (1/2/3/4)
✅ Form fields with validation
✅ Error messages on invalid input
✅ Next/Previous buttons
✅ Order summary sidebar (sticky)
✅ Real-time total calculations
✅ Radio button styling for shipping
✅ Payment method information
```

### Cart Page Elements:
```
✅ Product images
✅ Product names and quantities
✅ Unit prices and line totals
✅ Quantity increment/decrement
✅ Remove item button
✅ Subtotal, tax, shipping calculation
✅ Order summary
✅ "Proceed to Checkout" button
```

## 🐛 Troubleshooting

### Issue: "Unauthorized" error
- **Solution**: Make sure you're logged in before going to checkout
- Test credentials: Use Register page to create test account

### Issue: Cart items showing blank
- **Solution**: The API returns `productName` not `name`. This is already fixed in our code.

### Issue: Payment gateway not loading
- **Solution**: Check if backend endpoint is returning the gateway URL correctly
- Test endpoint: `curl -X POST http://localhost:5000/api/payments/initiate -H "Authorization: Bearer YOUR_TOKEN"`

### Issue: Checkout redirecting to login
- **Solution**: Auth check uses `useAuthStore().isAuthenticated`. Make sure you're authenticated.

### Issue: Total calculation wrong
- **Solution**: Verify in order summary sidebar:
  - Subtotal = sum of (price × quantity)
  - Tax = subtotal × 0.10
  - Shipping = selected method cost
  - Total = subtotal + tax + shipping

## 📊 Testing Checklist

- [ ] Add to cart works
- [ ] Cart displays correct items
- [ ] Subtotal calculates correctly
- [ ] Step 1 validation works (no empty fields)
- [ ] Step 2 shipping method selection works
- [ ] Step 3 displays correct order summary
- [ ] Step 4 shows payment info
- [ ] "Pay Now" sends POST to /api/payments/initiate
- [ ] Backend returns gateway URL
- [ ] Redirects to SSLCommerz sandbox
- [ ] Test card payment accepted
- [ ] Success page shows and clears cart
- [ ] Auto-redirects to home after 5 seconds
- [ ] Previous button works on all steps
- [ ] Total updates with shipping method change

## 🎯 Key Endpoints

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| POST | `/api/payments/initiate` | ✅ Bearer token | Create SSLCommerz payment session |
| GET | `/checkout/success` | ❌ Public | Payment success callback |
| GET | `/checkout/failed` | ❌ Public | Payment failure callback |

## 📝 Notes

- SSLCommerz test mode doesn't actually charge cards
- All transactions are test transactions
- Use sandbox credentials provided
- Success/failure URLs must be valid domain URLs (localhost works in sandbox)
- Transaction IDs include order ID and timestamp for tracking
- Cart is automatically cleared on successful payment
- Failed payments preserve cart for retry

## 🔐 Security Considerations

- Payment endpoint requires authentication (bearer token)
- Customer info validated before sending to SSLCommerz
- Transaction IDs include order identifier for tracking
- Success/fail pages are public (for callback redirects)
- Production: Add HTTPS, store validation, webhook verification

