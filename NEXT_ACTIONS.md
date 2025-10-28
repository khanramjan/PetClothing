# 🎯 Next Actions - Checkout Phase 2

## Immediate Tasks (Next 1-2 hours)

### 1. Stripe Setup
```bash
# Install Stripe packages
npm install @stripe/react-stripe-js @stripe/js
```

### 2. Create Stripe Provider Wrapper
File: `src/components/StripeProvider.tsx`
- Wrap app with StripeProvider
- Initialize Stripe key from environment
- Create reusable CardElement component

### 3. Implement Payment Form in Step 4
File: `src/pages/Checkout.tsx` - Step 4 enhancement
- Add CardElement from Stripe
- Handle card validation
- Create payment intent
- Submit payment via backend endpoint

### 4. Test End-to-End Flow
```bash
# Start dev servers
Terminal 1: npm run dev (frontend)
Terminal 2: cd backend && dotnet run (backend)

# Test checkout flow
- Add product to cart
- Go through all 4 steps
- Verify API calls in Network tab
- Check console for errors
```

## Environment Variables Needed

Create `.env.local` in frontend folder:
```env
VITE_API_URL=http://localhost:5000/api
VITE_STRIPE_PUBLIC_KEY=pk_test_YOUR_KEY_HERE
```

## Backend API Endpoints Ready

All 6 endpoints are ready to receive calls from frontend:

```
POST   /api/checkout/create-order          - Create order with all details
GET    /api/checkout/summary               - Get cart + addresses + shipping
POST   /api/checkout/calculate-tax         - Calculate tax for address
POST   /api/checkout/calculate-shipping    - Get shipping costs
GET    /api/checkout/shipping-methods      - List all shipping options
GET    /api/checkout/tax-rates             - List all tax rates
```

## State Management Already Implemented

All store actions ready to call:
- `fetchCheckoutSummary(token)` - Load all checkout data
- `calculateTax(addressId, token)` - Get tax amount
- `calculateShipping(shippingMethodId, token)` - Get shipping cost
- `getShippingMethods(token)` - List methods
- `getTaxRates(token)` - List tax rates  
- `createOrder(addressId, shippingMethodId, couponCode, token)` - Submit order

## File Structure Ready

```
frontend/
├── src/
│   ├── store/
│   │   ├── checkoutStore.ts        ✅ READY
│   │   └── authStore.ts             ✅ Ready
│   ├── pages/
│   │   ├── Checkout.tsx             ✅ READY (4-step)
│   │   └── Cart.tsx                 ✅ Ready
│   ├── components/
│   │   ├── StripeProvider.tsx       📝 TODO
│   │   └── CardElement.tsx          📝 TODO
│   ├── lib/
│   │   └── api.ts                   ✅ Ready
│   └── vite-env.d.ts                ✅ Ready
```

## Testing Checklist

- [ ] Stripe Elements loads without errors
- [ ] Card input accepts valid test cards
- [ ] Card validation shows errors
- [ ] Payment intent created successfully
- [ ] Order created in database
- [ ] Confirmation screen displays
- [ ] Order number generated correctly
- [ ] Email would be sent (check logs)

## Test Credit Cards (Stripe)

- Visa Success: 4242 4242 4242 4242
- Visa Declined: 4000 0000 0000 0002
- 3D Secure Required: 4000 0025 0000 3155
- Expiry: Any future date (e.g., 12/26)
- CVC: Any 3 digits (e.g., 123)

## Common Issues & Fixes

**Issue**: Stripe key not found
- Fix: Add VITE_STRIPE_PUBLIC_KEY to .env.local

**Issue**: CORS errors from API
- Fix: Ensure backend running on localhost:5000

**Issue**: Token expired
- Fix: Login again, token refreshed in localStorage

**Issue**: Payment fails with "Card declined"
- Fix: Check error message, use test cards above

---

**Estimated Time**: 1-2 hours for full Stripe integration
**Difficulty**: Medium (mostly connecting existing pieces)
**Priority**: 🔴 HIGH - Blocks revenue
