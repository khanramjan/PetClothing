# Quick Deployment Guide

## 🚀 Fastest Way to Deploy (Free)

### 1. Deploy Database (Railway - Free)

```bash
# Install Railway CLI
npm install -g @railway/cli

# Login
railway login

# Create new project
railway init

# Add PostgreSQL database
railway add

# Get connection string
railway variables
# Copy the DATABASE_URL
```

### 2. Update Backend Configuration

Replace in `backend/PetClothingShop.API/appsettings.Production.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "YOUR_RAILWAY_DATABASE_URL_HERE"
}
```

### 3. Deploy Backend (Railway)

```bash
cd backend/PetClothingShop.API

# Deploy to Railway
railway up

# Set environment variables in Railway dashboard
railway open
# Add: ASPNETCORE_ENVIRONMENT = Production
# Add: Jwt__Secret = (generate a secure 64-char key)
# Add: Backend__BaseUrl = https://your-app.railway.app
# Add: Frontend__Url = https://your-site.vercel.app
```

### 4. Run Database Migrations

```bash
# Connect to Railway database and run migrations
dotnet ef database update --connection "YOUR_RAILWAY_CONNECTION_STRING"
```

### 5. Deploy Frontend (Vercel - Free)

```bash
# Build frontend
cd frontend
npm run build

# Install Vercel CLI
npm install -g vercel

# Login and deploy
vercel login
vercel --prod

# In Vercel dashboard, add environment variable:
# VITE_API_URL = https://your-backend.railway.app/api
```

### 6. Configure CORS

Update `backend/PetClothingShop.API/Program.cs` CORS policy:
```csharp
policy.WithOrigins("https://your-frontend.vercel.app")
```

Redeploy backend:
```bash
railway up
```

### 7. Test Your Application

1. Visit your Vercel URL
2. Register a new user
3. Browse products
4. Make a test purchase
5. Check orders

## 🎉 Done!

Your application is now live and accessible worldwide!

### Important URLs:
- **Frontend**: https://your-app.vercel.app
- **Backend API**: https://your-app.railway.app
- **Database**: Managed by Railway

### Costs:
- Railway: $0 (Free tier: 500 hours/month)
- Vercel: $0 (Free tier: Unlimited bandwidth)
- Total: **$0/month**

### To Upgrade Later:
- Railway Starter: $5/month (better performance)
- Vercel Pro: $20/month (custom domain, analytics)

---

## Alternative: One-Command Deploy

### Using Render (All in one platform)

1. Create account at https://render.com
2. Click "New +" → "Blueprint"
3. Connect your GitHub repository
4. Render will auto-detect and deploy everything
5. Add environment variables in Render dashboard

That's it! ✨
