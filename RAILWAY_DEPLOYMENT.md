# Railway Backend Deployment Guide

## 🚀 Deploy to Railway (Free & Easy)

Railway offers a free tier perfect for hobby projects. Here's how to deploy:

### Step 1: Prepare Your Project

1. **Ensure your repository is clean:**
   ```powershell
   git status
   git add .
   git commit -m "Add OAuth and user sync implementation"
   git push
   ```

2. **Verify your `appsettings.json` has all configs:**
   - ✅ Database connection string
   - ✅ Supabase URL, JWT Secret, Issuer
   - ✅ JWT Secret, Issuer, Audience

### Step 2: Create Railway Account

1. Go to: https://railway.app
2. Click **"Sign Up"** (use GitHub for easy setup)
3. Connect your GitHub account
4. Authorize Railway

### Step 3: Deploy Backend

1. Click **"New Project"** → **"Deploy from GitHub"**
2. Select repository: `khanramjan/PetClothing`
3. Select branch: `master`
4. Railway will auto-detect it's a .NET project
5. Click **"Deploy"**

### Step 4: Configure Environment Variables

After deployment starts:

1. Go to **Project Settings** → **Variables**
2. Add these environment variables:

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=your-supabase-connection-string
Jwt__Secret=your-jwt-secret
Jwt__Issuer=PetClothingShop
Jwt__Audience=PetClothingShopClient
Supabase__Url=https://sdhpxjpbbevqhgiwtltz.supabase.co
Supabase__JwtSecret=your-supabase-jwt-secret
Supabase__Issuer=https://sdhpxjpbbevqhgiwtltz.supabase.co/auth/v1
```

### Step 5: Get Your Deployment URL

1. Go to **Deployments** tab
2. Copy your deployment URL (looks like: `https://your-app.railway.app`)
3. Update frontend `.env.local`:

```env
VITE_API_URL=https://your-app.railway.app/api
```

### Step 6: Test

1. Restart frontend dev server
2. Try signing in with email/password and Google OAuth
3. Check if requests go to your production backend

---

## 📦 Railway Free Tier Includes

- ✅ **$5/month free credit** (enough for hobby projects)
- ✅ **1 PostgreSQL database** (free)
- ✅ **Unlimited deployments**
- ✅ **Automatic HTTPS**
- ✅ **GitHub integration** (auto-deploy on push)
- ✅ **Custom domains**

---

## 🔄 Auto-Deploy on Push

After first deployment:

1. Any push to `master` branch
2. Railway automatically redeploys
3. New version live in ~2 minutes
4. No manual steps needed!

---

## 💡 Alternative Free Hosting Options

If Railway doesn't work for you:

### Option 1: **Render.com**
- Free tier with PostgreSQL
- https://render.com

### Option 2: **Fly.io**
- Free tier with generous limits
- https://fly.io

### Option 3: **Azure Free Credits**
- $200/month free for 12 months
- https://azure.microsoft.com/en-us/free/

---

## ✅ Deployment Checklist

- [ ] GitHub repository pushed
- [ ] Railway account created
- [ ] Backend connected to Railway
- [ ] Environment variables added
- [ ] Deployment URL working
- [ ] Frontend updated with API URL
- [ ] OAuth tested in production
- [ ] Database accessible from production

---

## 🐛 Common Issues

### "Deployment failed"
- Check Railway logs: click deployment → view logs
- Usually missing dependencies or config

### "Cannot connect to database"
- Verify connection string in Railway variables
- Ensure Supabase allows Railway IP
- Check database credentials

### "404 Not Found"
- Ensure API routes are correct
- Check CORS settings in Program.cs
- Verify deployment URL is correct

---

**You're ready to deploy! 🚀**
