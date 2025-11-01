# Azure Deployment - Quick Start (5 Minutes)

## 🎓 You Have GitHub Student Benefits!

These include **$100/month Azure credits** for 12 months, and free tier after that.

---

## ⚡ 5-Minute Setup

### Step 1: Activate Azure Student Benefits (1 min)

1. Go: https://azure.microsoft.com/en-us/free/students/
2. Click **"Activate now"**
3. Sign in with GitHub
4. Done! ✅

### Step 2: Create Azure Resources (2 min)

1. Go: https://portal.azure.com
2. Click **"Create a resource"**
3. Search **"App Service"** → Create
   - Name: `pet-clothing-shop-api`
   - Runtime: .NET 8
   - Plan: **Free (F1)**
4. Click Create

### Step 3: Configure App Settings (1 min)

1. In Azure Portal → Your App Service → Configuration
2. Add these settings:
   ```
   ConnectionStrings__DefaultConnection=your-supabase-connection
   Jwt__Secret=your-jwt-secret
   Supabase__Url=https://sdhpxjpbbevqhgiwtltz.supabase.co
   Supabase__JwtSecret=your-supabase-jwt-secret
   ```

### Step 4: Add GitHub Secrets (1 min)

1. Azure Portal → App Service → Download publish profile
2. GitHub repo → Settings → Secrets → New secret
   - Name: `AZURE_PUBLISH_PROFILE`
   - Value: (paste the XML you downloaded)
3. Save

### Step 5: Deploy! (Automatic)

```powershell
git add .
git commit -m "Add Azure deployment"
git push origin master
```

Watch GitHub Actions deploy automatically! 🚀

---

## ✅ Verify Deployment

1. Go to GitHub → Actions tab
2. Watch the workflow run
3. Wait for green checkmark ✅
4. Visit: `https://pet-clothing-shop-api.azurewebsites.net/api/products`
5. You should see JSON response!

---

## 🎯 Update Frontend API URL

Edit `frontend/.env.local`:

```env
VITE_API_URL=https://pet-clothing-shop-api.azurewebsites.net/api
```

Restart frontend, test Google OAuth! 🎉

---

## 💡 Key Benefits

✅ **Forever free** (after credits expire, F1 tier is free)
✅ **Auto-deploy** every push (GitHub Actions)
✅ **HTTPS included**
✅ **No credit card** needed (student account)
✅ **Perfect for** portfolio & learning

---

**That's it! Your backend is now deployed and auto-deploys on every push!** 🚀
