# Render Environment Variable Fix

## Problem
Render is still using the old connection string with the direct database (port 5432) causing 30+ second timeouts.

## Solution
Update the environment variable in Render Dashboard.

### Step-by-Step:

1. Go to **https://dashboard.render.com**
2. Click on **petclothing-1** service (backend)
3. Go to **Environment** tab
4. Look for `ConnectionStrings__DefaultConnection` variable
   - If it exists: Click it and update
   - If it doesn't exist: Click **Add Environment Variable**

5. **Set the value to:**
```
Host=aws-1-ap-southeast-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.sdhpxjpbbevqhgiwtltz;Password=@Rramjan_kh08;SSL Mode=Require;Trust Server Certificate=true;No Reset On Close=true;Multiplexing=true;Max Auto Prepare=20;Command Timeout=60;
```

6. Click **Save** at the bottom
7. Wait 2-3 minutes for auto-redeploy
8. Check logs in Render - should see `CommandTimeout='60'` instead of '30'

### What This Fixes:
- ✅ Uses connection pooler (port 6543) instead of direct connection (port 5432)
- ✅ Uses correct username format (postgres.sdhpxjpbbevqhgiwtltz)
- ✅ Increases timeout to 60 seconds for complex queries
- ✅ Enables connection pooling optimizations

### Verify It Works:
After redeploy, visit: **https://petclothing-1.onrender.com/api/products**

Should return JSON products list (not 500 error).

---

## Alternative: Use Render CLI

If you prefer command line:

```powershell
npm install -g render-cli
render login

# Get service ID
render services list

# Update variable (replace SERVICE_ID)
render env update SERVICE_ID ConnectionStrings__DefaultConnection "Host=aws-1-ap-southeast-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.sdhpxjpbbevqhgiwtltz;Password=@Rramjan_kh08;SSL Mode=Require;Trust Server Certificate=true;No Reset On Close=true;Multiplexing=true;Max Auto Prepare=20;Command Timeout=60;"
```
