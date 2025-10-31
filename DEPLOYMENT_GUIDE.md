# 🚀 Pet Clothing Shop - Deployment Guide

## Complete Hosting Guide for Production

---

## 📋 Table of Contents
1. [Hosting Options](#hosting-options)
2. [Database Setup](#database-setup)
3. [Backend Deployment](#backend-deployment)
4. [Frontend Deployment](#frontend-deployment)
5. [Environment Configuration](#environment-configuration)
6. [SSLCommerz Production Setup](#sslcommerz-production)
7. [Post-Deployment Checklist](#post-deployment-checklist)

---

## 🎯 Hosting Options

### **Recommended Hosting Stack:**

#### **Option 1: All-in-One (Easiest & Cost-Effective for Bangladesh)**
- **Backend + Database**: [Railway](https://railway.app) or [Render](https://render.com)
- **Frontend**: [Vercel](https://vercel.com) or [Netlify](https://netlify.com)
- **Database**: Railway PostgreSQL or Render PostgreSQL
- **File Storage**: AWS S3 or Cloudinary
- **Cost**: ~$5-10/month

#### **Option 2: Professional Setup**
- **Backend**: [DigitalOcean App Platform](https://www.digitalocean.com/products/app-platform) or Azure App Service
- **Database**: DigitalOcean Managed PostgreSQL or Azure PostgreSQL
- **Frontend**: Vercel or Netlify
- **File Storage**: DigitalOcean Spaces or AWS S3
- **Cost**: ~$15-25/month

#### **Option 3: Full Control (Most Flexible)**
- **VPS**: DigitalOcean Droplet ($4-6/month) or Linode
- **Database**: Self-hosted PostgreSQL on VPS
- **Web Server**: Nginx + PM2
- **Frontend**: Same VPS or separate CDN
- **Cost**: ~$4-10/month

---

## 💾 Database Setup for Remote Access

### **Current Issue:**
Your database connection is currently:
```
Host=localhost;Database=PetClothingShopDB;Username=postgres;Password=ramjankh08
```
This will NOT work when hosted because:
- `localhost` only refers to the local machine
- Remote users can't connect to your local PostgreSQL

### **Solution Options:**

#### **Option A: Managed PostgreSQL (Recommended)**

**1. Railway (Free Tier Available)**
```bash
# Steps:
1. Go to https://railway.app
2. Sign up with GitHub
3. Create New Project → Database → PostgreSQL
4. Copy the connection string provided
```

**2. Render (Free Tier Available)**
```bash
# Steps:
1. Go to https://render.com
2. Sign up with GitHub
3. New → PostgreSQL
4. Copy the External Database URL
```

**3. Supabase (Free & Based in Bangladesh-friendly)**
```bash
# Steps:
1. Go to https://supabase.com
2. Create new project
3. Get connection string from Settings → Database
```

#### **Option B: Self-Hosted on VPS**

**DigitalOcean Setup:**
```bash
# 1. Create Droplet (Ubuntu 22.04)
# 2. Install PostgreSQL
sudo apt update
sudo apt install postgresql postgresql-contrib -y

# 3. Configure PostgreSQL for remote access
sudo nano /etc/postgresql/14/main/postgresql.conf
# Change: listen_addresses = '*'

sudo nano /etc/postgresql/14/main/pg_hba.conf
# Add: host all all 0.0.0.0/0 md5

# 4. Create database and user
sudo -u postgres psql
CREATE DATABASE PetClothingShopDB;
CREATE USER petshop_user WITH PASSWORD 'your_strong_password';
GRANT ALL PRIVILEGES ON DATABASE PetClothingShopDB TO petshop_user;
\q

# 5. Restart PostgreSQL
sudo systemctl restart postgresql

# 6. Open firewall port
sudo ufw allow 5432/tcp
```

---

## 🔧 Backend Deployment

### **Step 1: Prepare Backend for Production**

Create `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_DB_HOST;Port=5432;Database=PetClothingShopDB;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;SSL Mode=Require"
  },
  "Jwt": {
    "Secret": "GENERATE_NEW_SECURE_SECRET_KEY_AT_LEAST_64_CHARACTERS_LONG",
    "Issuer": "PetClothingShop",
    "Audience": "PetClothingShopClient",
    "AccessTokenExpiryMinutes": "60",
    "RefreshTokenExpiryDays": "7"
  },
  "FileUpload": {
    "Path": "wwwroot/uploads",
    "MaxSizeInMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"]
  },
  "SSLCommerz": {
    "StoreId": "YOUR_PRODUCTION_STORE_ID",
    "StorePassword": "YOUR_PRODUCTION_PASSWORD",
    "ApiUrl": "https://securepay.sslcommerz.com/gwprocess/v4/api.php",
    "ValidationUrl": "https://securepay.sslcommerz.com/validator/api/validationserverAPI.php",
    "IsProduction": true
  },
  "Backend": {
    "BaseUrl": "https://your-backend-domain.com"
  },
  "Frontend": {
    "Url": "https://your-frontend-domain.com"
  }
}
```

### **Step 2: Deploy to Railway (Example)**

```bash
# 1. Install Railway CLI
npm i -g @railway/cli

# 2. Login
railway login

# 3. Initialize project
cd backend/PetClothingShop.API
railway init

# 4. Add PostgreSQL
railway add

# 5. Deploy
railway up
```

### **Step 3: Deploy to Render (Example)**

Create `render.yaml` in backend folder:

```yaml
services:
  - type: web
    name: pet-clothing-api
    env: dotnet
    buildCommand: dotnet publish -c Release -o out
    startCommand: dotnet out/PetClothingShop.API.dll
    envVars:
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: ConnectionStrings__DefaultConnection
        fromDatabase:
          name: petshop-db
          property: connectionString
      - key: Jwt__Secret
        generateValue: true
      - key: Backend__BaseUrl
        value: https://your-backend.onrender.com
      - key: Frontend__Url
        value: https://your-frontend.vercel.app

databases:
  - name: petshop-db
    databaseName: PetClothingShopDB
    user: petshop_user
```

### **Step 4: Run Database Migrations**

```bash
# After deploying, run migrations
dotnet ef database update --project PetClothingShop.Infrastructure --startup-project PetClothingShop.API

# Or connect to production database and run migrations
$env:ConnectionStrings__DefaultConnection="YOUR_PRODUCTION_CONNECTION_STRING"
dotnet ef database update
```

---

## 🎨 Frontend Deployment

### **Step 1: Update API Configuration**

Update `frontend/src/lib/api.ts` or wherever you have API base URL:

```typescript
// Create .env.production file
VITE_API_URL=https://your-backend-domain.com/api
VITE_SSLCOMMERZ_STORE_ID=your_production_store_id
```

### **Step 2: Build for Production**

```bash
cd frontend
npm run build
# Creates 'dist' folder with optimized build
```

### **Step 3: Deploy to Vercel**

```bash
# Install Vercel CLI
npm i -g vercel

# Login and deploy
cd frontend
vercel login
vercel --prod

# Or use Vercel Dashboard:
# 1. Import Git Repository
# 2. Set build command: npm run build
# 3. Set output directory: dist
# 4. Add environment variables
```

### **Step 4: Deploy to Netlify**

```bash
# Install Netlify CLI
npm i -g netlify-cli

# Login and deploy
cd frontend
netlify login
netlify deploy --prod

# Or use Netlify Dashboard:
# 1. Drag and drop 'dist' folder
# 2. Or connect Git repository
```

---

## 🔐 Environment Configuration

### **Backend Environment Variables:**

```bash
# Required for production
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=YOUR_DB_CONNECTION_STRING
Jwt__Secret=YOUR_SECRET_KEY
Backend__BaseUrl=https://your-api.com
Frontend__Url=https://your-site.com
SSLCommerz__StoreId=YOUR_STORE_ID
SSLCommerz__StorePassword=YOUR_PASSWORD
SSLCommerz__IsProduction=true
```

### **Frontend Environment Variables:**

Create `.env.production`:
```bash
VITE_API_URL=https://your-backend.com/api
VITE_SSLCOMMERZ_STORE_ID=your_production_store_id
```

---

## 💳 SSLCommerz Production Setup

### **Switch from Sandbox to Production:**

1. **Get Production Credentials:**
   - Contact SSLCommerz: integration@sslcommerz.com
   - Request production store credentials
   - Complete KYC verification
   - Get Store ID and Store Password

2. **Update Configuration:**
   ```json
   "SSLCommerz": {
     "StoreId": "your_production_store_id",
     "StorePassword": "your_production_password",
     "ApiUrl": "https://securepay.sslcommerz.com/gwprocess/v4/api.php",
     "ValidationUrl": "https://securepay.sslcommerz.com/validator/api/validationserverAPI.php",
     "IsProduction": true
   }
   ```

3. **Update Callback URLs:**
   - Success: `https://your-domain.com/payment/success`
   - Fail: `https://your-domain.com/payment/fail`
   - Cancel: `https://your-domain.com/payment/cancel`
   - IPN: `https://your-backend.com/api/payments/ipn`

---

## ✅ Post-Deployment Checklist

### **Database:**
- [ ] PostgreSQL database created
- [ ] Migrations applied successfully
- [ ] Seed data loaded (products, categories)
- [ ] Database connection string secure
- [ ] SSL/TLS enabled for database connection
- [ ] Regular backups configured

### **Backend:**
- [ ] API deployed and accessible
- [ ] Health check endpoint working
- [ ] CORS configured for frontend domain
- [ ] Environment variables set
- [ ] File upload working
- [ ] Logs configured
- [ ] Rate limiting active
- [ ] HTTPS enabled

### **Frontend:**
- [ ] Build deployed successfully
- [ ] API URL pointing to production backend
- [ ] Environment variables set
- [ ] HTTPS enabled
- [ ] Custom domain configured (optional)
- [ ] CDN enabled

### **SSLCommerz:**
- [ ] Production credentials obtained
- [ ] Callback URLs updated
- [ ] Test transactions working
- [ ] IPN endpoint configured
- [ ] Payment validation working

### **Security:**
- [ ] Strong JWT secret key
- [ ] Strong database password
- [ ] Secrets not in source code
- [ ] HTTPS everywhere
- [ ] CORS properly configured
- [ ] Rate limiting active
- [ ] SQL injection protection (Entity Framework)

### **Testing:**
- [ ] User registration works
- [ ] User login works
- [ ] Product browsing works
- [ ] Cart functionality works
- [ ] Checkout process works
- [ ] Payment processing works
- [ ] Order creation works
- [ ] Profile management works
- [ ] Admin panel works

---

## 🆘 Common Issues & Solutions

### **1. Database Connection Fails**
```bash
# Check connection string format
Host=hostname;Port=5432;Database=dbname;Username=user;Password=pass;SSL Mode=Require

# For Supabase/Railway/Render, they provide full connection string
```

### **2. CORS Errors**
```csharp
// Update Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://your-frontend.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### **3. File Upload Issues**
```bash
# If using Railway/Render (ephemeral filesystem)
# Use external storage:
# - AWS S3
# - Cloudinary
# - DigitalOcean Spaces
```

### **4. Database Migrations**
```bash
# Run migrations after deployment
dotnet ef database update --connection "YOUR_CONNECTION_STRING"
```

---

## 💰 Cost Estimate (Monthly)

### **Budget Option:**
- Render Web Service (Free tier): $0
- Render PostgreSQL (Free tier): $0
- Vercel (Free tier): $0
- **Total: $0/month** (with limitations)

### **Starter Option:**
- Railway (Starter): $5
- Vercel (Free): $0
- **Total: $5/month**

### **Production Option:**
- Render Web Service: $7
- Render PostgreSQL: $7
- Vercel Pro: $20
- **Total: $34/month**

---

## 📞 Support & Resources

### **Bangladesh-Specific:**
- **SSLCommerz Support**: integration@sslcommerz.com, +880-2-8836404
- **Hosting**: Railway, Render (international credit card needed)
- **Local Hosting**: Contact local ISPs for VPS (DataCare, Dhaka Web Host)

### **Documentation:**
- [Railway Docs](https://docs.railway.app)
- [Render Docs](https://render.com/docs)
- [Vercel Docs](https://vercel.com/docs)
- [SSLCommerz Docs](https://developer.sslcommerz.com)

---

## 🎉 Quick Start (Fastest Way to Deploy)

```bash
# 1. Create accounts (all free tiers)
- Railway.app
- Vercel.com

# 2. Deploy Database (Railway)
railway login
railway init
railway add # Select PostgreSQL
# Copy connection string

# 3. Deploy Backend (Railway)
cd backend/PetClothingShop.API
railway up

# 4. Run migrations
dotnet ef database update --connection "YOUR_RAILWAY_DB_CONNECTION"

# 5. Deploy Frontend (Vercel)
cd frontend
vercel login
vercel --prod

# 6. Update frontend API URL in Vercel dashboard
# Environment Variables → VITE_API_URL → https://your-backend.railway.app/api

# 7. Test everything!
```

---

## ✨ Next Steps After Deployment

1. **Monitor Application**
   - Set up logging (Serilog to file/database)
   - Monitor errors and performance
   - Check database size and connections

2. **Optimize Performance**
   - Enable caching
   - Optimize images
   - Add CDN for static assets

3. **Scale as Needed**
   - Upgrade database if needed
   - Add load balancer if traffic grows
   - Consider microservices for large scale

4. **Marketing**
   - Set up Google Analytics
   - Configure SEO
   - Add social media links

---

**🎯 Your application is ready to serve customers worldwide! The database will work perfectly for remote users once you follow this guide.**
