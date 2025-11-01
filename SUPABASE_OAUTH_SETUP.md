# Supabase OAuth Setup Guide

## 🎯 Overview
This guide will help you set up Google OAuth authentication using Supabase Auth in your Pet Clothing Shop application.

## 📋 Prerequisites
- Supabase project created at https://app.supabase.com
- Google Cloud Console account for OAuth credentials

---

## Part 1: Configure Supabase

### 1. Get Supabase Credentials

1. Go to your Supabase project dashboard: https://app.supabase.com/project/_/settings/api
2. Copy the following values:
   - **Project URL** (looks like: `https://xxxxxxxxxxxxx.supabase.co`)
   - **anon/public key** (starts with `eyJ...`)
   - **JWT Secret** (from Settings > API > JWT Settings)

### 2. Enable Google OAuth in Supabase

1. Go to **Authentication** > **Providers** in your Supabase dashboard
2. Find **Google** and click to expand
3. Enable **Google provider**
4. You'll need to add Google OAuth credentials (see Part 2)

---

## Part 2: Set Up Google OAuth

### 1. Create Google OAuth Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Go to **APIs & Services** > **Credentials**
4. Click **Create Credentials** > **OAuth 2.0 Client ID**
5. Configure the consent screen if prompted
6. Select **Web application** as application type
7. Add authorized redirect URIs:
   ```
   https://your-project-ref.supabase.co/auth/v1/callback
   http://localhost:5173  (for local development)
   ```
8. Click **Create** and copy:
   - **Client ID**
   - **Client Secret**

### 2. Add Google Credentials to Supabase

1. Go back to Supabase **Authentication** > **Providers** > **Google**
2. Paste your **Client ID** and **Client Secret**
3. Click **Save**

---

## Part 3: Configure Frontend

### 1. Create `.env.local` file

Create a file named `.env.local` in the `frontend` directory:

```env
# Supabase Configuration
VITE_SUPABASE_URL=https://your-project-ref.supabase.co
VITE_SUPABASE_ANON_KEY=your-anon-key-here

# Backend API
VITE_API_URL=http://localhost:5000/api
```

Replace:
- `your-project-ref` with your actual Supabase project reference
- `your-anon-key-here` with your Supabase anon/public key

### 2. Test Frontend OAuth

The frontend is already configured with:
- ✅ Supabase client (`src/lib/supabaseClient.ts`)
- ✅ Google OAuth button in Login page
- ✅ Google OAuth button in Register page
- ✅ Auth state management in `authStore`
- ✅ Auto-sync with Supabase sessions

---

## Part 4: Configure Backend

### 1. Update `appsettings.json`

Edit `backend/PetClothingShop.API/appsettings.json` and update the Supabase section:

```json
"Supabase": {
  "Url": "https://your-project-ref.supabase.co",
  "JwtSecret": "your-jwt-secret-here",
  "Issuer": "https://your-project-ref.supabase.co/auth/v1"
}
```

Replace:
- `your-project-ref` with your actual Supabase project reference
- `your-jwt-secret-here` with your JWT Secret from Supabase Settings > API

### 2. JWT Validation

The backend is already configured to:
- ✅ Validate both internal JWT tokens and Supabase JWT tokens
- ✅ Auto-detect token type based on issuer
- ✅ Apply different validation rules for each token type

---

## Part 5: Database Sync (Optional)

### Option A: Supabase Webhooks (Recommended)

Create a webhook in Supabase to sync users to your database:

1. Go to **Database** > **Webhooks** in Supabase
2. Create a new webhook for `auth.users` table
3. Trigger on: `INSERT`
4. Webhook URL: `https://your-backend-url/api/auth/supabase-webhook`
5. Secret: Generate a random secret

### Option B: Client-Side Sync

After successful Google OAuth, call your backend API to create/sync the user:

```typescript
const { data: { session } } = await supabase.auth.getSession();
if (session) {
  // Call your backend to sync user
  await api.post('/auth/sync-supabase-user', {
    email: session.user.email,
    firstName: session.user.user_metadata.first_name,
    lastName: session.user.user_metadata.last_name
  });
}
```

---

## 🧪 Testing

### 1. Start the Application

**Backend:**
```powershell
cd backend\PetClothingShop.API
dotnet run
```

**Frontend:**
```powershell
cd frontend
npm run dev
```

### 2. Test Google OAuth

1. Open http://localhost:5173
2. Go to **Login** or **Register** page
3. Click **Continue with Google** button
4. Sign in with your Google account
5. You should be redirected back to the app and logged in

### 3. Verify Authentication

- Check that you're logged in (user info in navbar)
- Try accessing protected routes (cart, profile, orders)
- Check browser console for any errors
- Inspect localStorage for `accessToken`

---

## 🔍 Troubleshooting

### "Missing Supabase environment variables"
- Ensure `.env.local` exists in `frontend` directory
- Restart the Vite dev server after creating `.env.local`

### Google OAuth redirect not working
- Check authorized redirect URIs in Google Cloud Console
- Ensure Supabase callback URL is added: `https://your-project.supabase.co/auth/v1/callback`

### Backend JWT validation failing
- Verify `Supabase:JwtSecret` in `appsettings.json` matches your Supabase JWT Secret
- Check backend logs for detailed error messages

### User not syncing to database
- Implement one of the sync strategies (webhooks or client-side)
- Check backend logs for errors during sync

---

## 📚 Resources

- [Supabase Auth Documentation](https://supabase.com/docs/guides/auth)
- [Google OAuth 2.0 Guide](https://developers.google.com/identity/protocols/oauth2)
- [Supabase Google OAuth Setup](https://supabase.com/docs/guides/auth/social-login/auth-google)

---

## ✅ Checklist

- [ ] Supabase project created
- [ ] Supabase credentials copied
- [ ] Google OAuth credentials created
- [ ] Google OAuth configured in Supabase
- [ ] Frontend `.env.local` created and configured
- [ ] Backend `appsettings.json` updated
- [ ] Application tested with Google sign-in
- [ ] User sync strategy implemented (optional)

---

## 🎉 You're All Set!

Your Pet Clothing Shop now supports Google OAuth via Supabase Auth! Users can sign in with their Google accounts on both Login and Register pages.
