# Quick Start - Google OAuth Setup

## ⚡ Fast Setup (5 Minutes)

### 1️⃣ Get Supabase Credentials (2 min)

1. Go to https://app.supabase.com/project/_/settings/api
2. Copy these 3 values:
   - **Project URL**: `https://xxxxx.supabase.co`
   - **anon key**: `eyJxxx...` (long string)
   - **JWT Secret**: From "JWT Settings" section

### 2️⃣ Configure Frontend (1 min)

Create `frontend/.env.local`:
```env
VITE_SUPABASE_URL=https://xxxxx.supabase.co
VITE_SUPABASE_ANON_KEY=eyJxxx...
VITE_API_URL=http://localhost:5000/api
```

### 3️⃣ Configure Backend (1 min)

Edit `backend/PetClothingShop.API/appsettings.json`:
```json
"Supabase": {
  "Url": "https://xxxxx.supabase.co",
  "JwtSecret": "your-jwt-secret-here",
  "Issuer": "https://xxxxx.supabase.co/auth/v1"
}
```

### 4️⃣ Enable Google in Supabase (1 min)

1. Go to https://app.supabase.com/project/_/auth/providers
2. Find "Google" → Click "Enable"
3. For now, you can use the default Google OAuth credentials provided by Supabase for testing

### 5️⃣ Test It! (30 sec)

```powershell
# Terminal 1 - Backend
cd backend\PetClothingShop.API
dotnet run

# Terminal 2 - Frontend
cd frontend
npm run dev
```

Open http://localhost:5173/login and click "Continue with Google"! 🎉

---

## 🔧 Production Setup

For production, you'll need your own Google OAuth credentials:

1. Go to https://console.cloud.google.com/
2. Create OAuth 2.0 Client ID
3. Add redirect URI: `https://xxxxx.supabase.co/auth/v1/callback`
4. Copy Client ID and Secret to Supabase Auth > Providers > Google

See `SUPABASE_OAUTH_SETUP.md` for detailed instructions.

---

## 📋 What You Get

- ✅ "Continue with Google" button on Login page
- ✅ "Continue with Google" button on Register page
- ✅ Automatic session management
- ✅ Persistent login (stays logged in after refresh)
- ✅ Secure JWT token validation
- ✅ Works with your existing email/password auth

---

## 🐛 Common Issues

**"Missing Supabase environment variables"**
- Make sure `.env.local` exists in `frontend` folder
- Restart dev server: `Ctrl+C` then `npm run dev`

**Can't click Google button**
- Check browser console for errors
- Verify `.env.local` has correct values

**Redirects but not logged in**
- Check Supabase project URL in both `.env.local` and `appsettings.json`
- Verify JWT Secret is correct

---

## 📚 Full Documentation

- `SUPABASE_OAUTH_SETUP.md` - Complete setup guide
- `OAUTH_IMPLEMENTATION.md` - Technical implementation details
