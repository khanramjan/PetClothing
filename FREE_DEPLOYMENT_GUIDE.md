# 🚀 FREE DEPLOYMENT GUIDE - Pet Clothing Shop

## 🎯 Easiest & Totally FREE Options

### Option 1: Railway (Recommended - You Already Have DB) ⭐⭐⭐

#### Why Railway?
- ✅ You already have PostgreSQL database there
- ✅ Supports .NET Core perfectly
- ✅ Free tier: 512MB RAM, 1GB storage
- ✅ Automatic deployments from GitHub
- ✅ Built-in SSL certificates

#### Backend Deployment (Railway)

1. **Go to Railway Dashboard**
   - Visit: https://railway.app/dashboard
   - Your project should already be there

2. **Add Backend Service**
   - Click "New" → "GitHub Repo"
   - Select your `PetClothing` repository
   - Choose "Deploy from GitHub"

3. **Configure Environment Variables**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:$PORT
   ConnectionStrings__DefaultConnection=Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Prefer;Trust Server Certificate=true
   Jwt__Secret=YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm
   Backend__BaseUrl=https://your-backend-domain.up.railway.app
   Frontend__Url=https://your-frontend-domain.vercel.app
   SSLCommerz__StoreId=your_production_store_id
   SSLCommerz__StorePassword=your_production_password
   SSLCommerz__IsProduction=true
   ```

4. **Set Build Settings**
   - Build Command: `dotnet publish backend/PetClothingShop.API -c Release -o out`
   - Start Command: `cd out && dotnet PetClothingShop.API.dll`

5. **Deploy**
   - Railway will auto-deploy from your GitHub repo
   - Get your backend URL: `https://your-project-name.up.railway.app`

#### Frontend Deployment (Vercel)

1. **Go to Vercel**
   - Visit: https://vercel.com
   - Sign up with GitHub

2. **Import Repository**
   - Click "New Project"
   - Import your `PetClothing` repository
   - Configure project:
     - **Root Directory:** `frontend`
     - **Build Command:** `npm run build`
     - **Output Directory:** `dist`

3. **Environment Variables**
   ```
   VITE_API_URL=https://your-backend-domain.up.railway.app
   ```

4. **Deploy**
   - Vercel will auto-deploy
   - Get your frontend URL: `https://your-project-name.vercel.app`

---

### Option 2: Render (Alternative Free Option)

#### Backend (Render)
1. **Go to Render Dashboard**
   - Visit: https://dashboard.render.com
   - Sign up with GitHub

2. **Create Web Service**
   - Click "New" → "Web Service"
   - Connect your GitHub repo
   - Configure:
     - **Runtime:** `Docker`
     - **Root Directory:** `backend/PetClothingShop.API`
     - **Dockerfile Path:** `./Dockerfile` (we'll create this)

3. **Environment Variables** (same as Railway)

4. **Create Dockerfile**
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
   WORKDIR /app
   EXPOSE 80

   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY ["PetClothingShop.API.csproj", "."]
   RUN dotnet restore "./PetClothingShop.API.csproj"
   COPY . .
   WORKDIR "/src/."
   RUN dotnet build "PetClothingShop.API.csproj" -c Release -o /app/build

   FROM build AS publish
   RUN dotnet publish "PetClothingShop.API.csproj" -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "PetClothingShop.API.dll"]
   ```

#### Database (Render PostgreSQL)
1. **Create Database**
   - "New" → "PostgreSQL"
   - Free tier available
   - Get connection string

#### Frontend (Vercel) - Same as above

---

### Option 3: Fly.io (Another Free Option)

#### Backend (Fly.io)
1. **Install Fly CLI**
   ```bash
   # Windows
   powershell -Command "iwr https://fly.io/install.ps1 | iex"
   ```

2. **Login & Initialize**
   ```bash
   fly auth login
   cd backend/PetClothingShop.API
   fly launch
   ```

3. **Configure fly.toml**
   ```toml
   app = "pet-clothing-shop"
   primary_region = "sin"

   [build]
   builder = "heroku/buildpacks:20"

   [env]
   ASPNETCORE_ENVIRONMENT = "Production"
   ASPNETCORE_URLS = "http://0.0.0.0:$PORT"

   [[services]]
   internal_port = 8080
   protocol = "tcp"

   [[services.ports]]
   handlers = ["http"]
   port = "80"
   ```

4. **Set Secrets**
   ```bash
   fly secrets set ConnectionStrings__DefaultConnection="your_connection_string"
   fly secrets set Jwt__Secret="your_jwt_secret"
   ```

5. **Deploy**
   ```bash
   fly deploy
   ```

---

## 🎯 RECOMMENDED QUICK DEPLOYMENT

### Step 1: Backend on Railway
1. Go to https://railway.app/dashboard
2. Click "New" → "GitHub Repo"
3. Select your repository
4. Add environment variables (copy from above)
5. Deploy

### Step 2: Frontend on Vercel
1. Go to https://vercel.com
2. Click "New Project"
3. Import your repository
4. Set root directory to `frontend`
5. Add `VITE_API_URL` environment variable
6. Deploy

### Step 3: Update Frontend URL
- Go back to Railway
- Update `Frontend__Url` environment variable with your Vercel URL

---

## 🔧 Environment Variables Reference

### Backend (Railway/Render/Fly.io)
```bash
# Database
ConnectionStrings__DefaultConnection=Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Prefer;Trust Server Certificate=true

# JWT
Jwt__Secret=YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm

# URLs
Backend__BaseUrl=https://your-backend.up.railway.app
Frontend__Url=https://your-frontend.vercel.app

# SSLCommerz (Production)
SSLCommerz__StoreId=your_production_store_id
SSLCommerz__StorePassword=your_production_password
SSLCommerz__IsProduction=true
```

### Frontend (Vercel)
```bash
VITE_API_URL=https://your-backend.up.railway.app
```

---

## ✅ Free Tier Limits

| Service | Free Limits | Notes |
|---------|-------------|-------|
| **Railway** | 512MB RAM, 1GB storage, 100 hours/month | Perfect for small apps |
| **Vercel** | Unlimited bandwidth, 100GB/month | Great for static sites |
| **Render** | 750 hours/month, 1GB storage | Good alternative |
| **Fly.io** | 3 shared CPUs, 256MB RAM, 160GB outbound | Limited but works |

---

## 🚀 Quick Start Commands

### If you want to deploy manually:

```bash
# Backend (Railway CLI if installed)
railway login
railway link
railway up

# Frontend (Vercel CLI if installed)
vercel --prod
```

---

## 🔍 Testing Your Deployment

1. **Backend Health Check**
   ```
   GET https://your-backend.up.railway.app/api/products
   ```

2. **Frontend Access**
   ```
   https://your-frontend.vercel.app
   ```

3. **Database Connection**
   - Check Railway dashboard for database status

---

## 🆘 Troubleshooting

### Backend Not Starting?
- Check environment variables in Railway dashboard
- Verify database connection string
- Check Railway build logs

### Frontend Not Loading?
- Check `VITE_API_URL` environment variable
- Verify CORS settings in backend
- Check Vercel build logs

### Database Connection Issues?
- Verify Railway database is running
- Check connection string format
- Ensure SSL settings are correct

---

## 💡 Pro Tips

1. **Custom Domain**: Both Railway and Vercel support custom domains for free
2. **Monitoring**: Use Railway's built-in logs and metrics
3. **Backups**: Railway automatically backs up your database
4. **Scaling**: Free tiers are sufficient for development/testing

---

## 🎉 You're Done!

Your Pet Clothing Shop will be live at:
- **Frontend**: `https://your-project.vercel.app`
- **Backend**: `https://your-project.up.railway.app`

Total Cost: **$0** 🎯