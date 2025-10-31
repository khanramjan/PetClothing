# Security Configuration Guide

## ✅ Security Improvements Applied

### 1. **Sensitive Files Protected**
- ✅ `appsettings.Production.json` added to `.gitignore` (contains Railway credentials)
- ✅ `appsettings.json` reverted to local development settings
- ✅ Railway connection string removed from version control

### 2. **Environment-Based Configuration**

#### **Development (Local)**
- Uses `appsettings.json`
- Local PostgreSQL database
- Run: `dotnet run`

#### **Production (Railway)**
- Uses `appsettings.Production.json` or environment variables
- Railway PostgreSQL database
- Run: `.\start-railway-backend.ps1`

## 🚀 How to Start Backend

### Option 1: Local Development
```powershell
cd backend\PetClothingShop.API
dotnet run
```
Uses local database from `appsettings.json`

### Option 2: Railway Database (Recommended)
```powershell
.\start-railway-backend.ps1
```
Uses Railway database with environment variables

### Option 3: Manual Environment Variables
```powershell
$env:ConnectionStrings__DefaultConnection="Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Prefer;Trust Server Certificate=true"
$env:ASPNETCORE_ENVIRONMENT="Production"
cd backend\PetClothingShop.API
dotnet run --urls "http://localhost:5000"
```

## 🔒 Security Best Practices

### Files to NEVER Commit:
- ❌ `appsettings.Production.json` (contains production credentials)
- ❌ `.env` files with real credentials
- ❌ Any file with database passwords

### Files Safe to Commit:
- ✅ `appsettings.json` (with generic/local settings)
- ✅ `.env.example` (template with placeholder values)
- ✅ `start-railway-backend.ps1` (convenience script)

## 📝 Configuration Files

### `appsettings.json` (Development)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=PetClothingShopDB;..."
  }
}
```
- Used for local development
- Safe to commit (no sensitive data)

### `appsettings.Production.json` (Production)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=mainline.proxy.rlwy.net;Port=47346;..."
  }
}
```
- Used for Railway/production
- **NEVER commit** (contains real credentials)
- Added to `.gitignore`

### Environment Variables (Preferred for Production)
```bash
ConnectionStrings__DefaultConnection=Host=...
ASPNETCORE_ENVIRONMENT=Production
```
- Most secure method
- Use in production deployments
- Railway/cloud platforms support this

## 🛡️ Current Security Status

| Item | Status | Description |
|------|--------|-------------|
| Railway credentials in git | ✅ Protected | Added to `.gitignore` |
| Development config | ✅ Safe | Uses local database |
| Production config | ✅ Separated | Environment-based |
| Startup script | ✅ Created | `start-railway-backend.ps1` |
| Example config | ✅ Created | `.env.example` |

## 🔄 Migration Guide

### If You Already Committed Sensitive Files:

1. **Remove from Git History:**
```bash
git filter-branch --force --index-filter \
  "git rm --cached --ignore-unmatch backend/PetClothingShop.API/appsettings.Production.json" \
  --prune-empty --tag-name-filter cat -- --all
```

2. **Force Push (WARNING: Rewrites history):**
```bash
git push origin --force --all
```

3. **Rotate Credentials:**
   - Change Railway database password
   - Update JWT secret
   - Update SSLCommerz credentials

### Better Alternative:
If you haven't pushed yet, just commit the `.gitignore` changes and the sensitive files won't be tracked.

## 📦 Deployment Checklist

- [ ] Verify `appsettings.Production.json` is in `.gitignore`
- [ ] Test backend starts with `start-railway-backend.ps1`
- [ ] Verify Railway database connection works
- [ ] Test signup creates user in Railway database
- [ ] Confirm admin login works
- [ ] Test payment flow with Railway database

## 🆘 Troubleshooting

### Backend Not Connecting to Railway?
```powershell
# Stop all dotnet processes
Stop-Process -Name "dotnet" -Force -ErrorAction SilentlyContinue

# Start with Railway connection
.\start-railway-backend.ps1
```

### Still Using Local Database?
Check that `ASPNETCORE_ENVIRONMENT=Production` is set, or the environment variable is properly configured.

### Lost appsettings.Production.json?
You can recreate it using the Railway credentials:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Prefer;Trust Server Certificate=true"
  }
}
```

## 📚 Additional Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Railway Environment Variables](https://docs.railway.app/develop/variables)
- [.NET Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
