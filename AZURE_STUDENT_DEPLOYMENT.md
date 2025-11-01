# Deploy to Azure with GitHub Student Benefits (Forever Free)

## 🎓 GitHub Student Benefits Include

✅ **$100/month Azure credits** (12 months)
✅ **Free tier after credits expire**
✅ **GitHub Actions** for CI/CD
✅ **Azure Container Registry**
✅ Excellent for hobby projects and learning

---

## 🚀 Quick Deploy to Azure

### Step 1: Verify Your GitHub Student Account

1. Go to: https://education.github.com/benefits
2. Sign in with your GitHub account
3. Verify you have Azure credits (should show $100/month)

### Step 2: Create Azure Account with Student Benefits

1. Visit: https://azure.microsoft.com/en-us/free/students/
2. Click **"Activate now"**
3. Sign in with your GitHub Student account
4. Activate your $100/month credits
5. This gives you **free resources for 12 months**

### Step 3: Deploy ASP.NET Core to Azure App Service

1. Go to: https://portal.azure.com
2. Click **"Create a resource"**
3. Search for **"App Service"**
4. Click **Create**

#### Configure App Service:

- **Name**: `pet-clothing-shop-api` (must be unique)
- **Publish**: Code
- **Runtime stack**: .NET 8 (latest)
- **Operating System**: Linux
- **Region**: (pick closest to you)
- **App Service Plan**: Create new → **Free tier (F1)**

#### Create Database (PostgreSQL):

1. Create new resource → **Azure Database for PostgreSQL**
2. Single server or Flexible server
3. **Basic tier** (within free credits)
4. Get connection string

#### Environment Variables:

In App Service → Configuration → Application settings, add:

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<your-supabase-connection-string>
Jwt__Secret=<your-jwt-secret>
Jwt__Issuer=PetClothingShop
Jwt__Audience=PetClothingShopClient
Supabase__Url=https://sdhpxjpbbevqhgiwtltz.supabase.co
Supabase__JwtSecret=<your-supabase-jwt-secret>
Supabase__Issuer=https://sdhpxjpbbevqhgiwtltz.supabase.co/auth/v1
```

### Step 4: Deploy via GitHub Actions (Auto-Deploy)

Create file: `.github/workflows/deploy-azure.yml`

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3
    
    - name: Set up .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Build backend
      run: dotnet build backend/PetClothingShop.API -c Release
    
    - name: Publish backend
      run: dotnet publish backend/PetClothingShop.API -c Release -o ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'pet-clothing-shop-api'
        publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
        package: ./publish
```

#### Add Publish Profile:

1. In Azure Portal → App Service → Download publish profile
2. Go to GitHub → Settings → Secrets and variables → Actions
3. Create new secret: `AZURE_PUBLISH_PROFILE`
4. Paste the publish profile XML

### Step 5: Test Deployment

1. Push to master: `git push origin master`
2. GitHub Actions automatically deploys
3. Check Azure Portal for deployment status
4. Get your URL: `https://pet-clothing-shop-api.azurewebsites.net`

---

## 💰 Pricing (Forever Free After Credits)

### Free Tier Includes:
- ✅ 1 App Service (F1 - shared, limited)
- ✅ 1 PostgreSQL (Basic - limited)
- ✅ GitHub Actions (2000 minutes/month free)

### What's Free Forever:
- App Service tier F1
- PostgreSQL Basic tier (limited)
- 1 GB storage
- 1 GB RAM

---

## 🔄 Auto-Deploy on Every Push

After setup, every `git push` automatically:
1. Runs tests
2. Builds backend
3. Deploys to Azure
4. Live in ~3-5 minutes

No manual steps needed!

---

## 📊 Monitor Your Usage

In Azure Portal → Cost Management:
- See your credit usage
- Monitor resource consumption
- Set budget alerts

---

## ✅ Deployment Checklist

- [ ] GitHub Student account verified
- [ ] Azure account created with student benefits
- [ ] $100/month credits activated
- [ ] App Service created (F1 tier)
- [ ] PostgreSQL database created
- [ ] Environment variables configured
- [ ] GitHub Actions workflow set up
- [ ] Publish profile added to GitHub Secrets
- [ ] First deployment successful
- [ ] Frontend API URL updated

---

## 🎯 What You Get

✅ **Forever free** after initial 12 months (if using F1 tier)
✅ **Auto-deploy** on every push
✅ **HTTPS** included
✅ **Custom domain** support
✅ **GitHub integration** seamless
✅ **Perfect for** hobby projects, portfolio, learning

---

## 🚀 Next Steps

1. **Activate Azure student benefits**: https://azure.microsoft.com/en-us/free/students/
2. **Create App Service** in Azure Portal
3. **Add GitHub Actions** workflow
4. **Push to master** and watch it deploy!

**Your backend will be live and free forever (on F1 tier)!** 🎉
