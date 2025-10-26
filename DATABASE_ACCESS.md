# Database Access Guide

## Method 1: Using pgAdmin 4 (Easiest - GUI)

### Step 1: Open pgAdmin 4
1. Press `Windows Key` and search for **"pgAdmin 4"**
2. Click to open it
3. Wait for it to load in your browser

### Step 2: Connect to Server
1. In the left panel, expand **Servers**
2. Click on **PostgreSQL 18**
3. Enter password: `ramjankh08`
4. Click OK

### Step 3: Access Your Database
1. Expand **PostgreSQL 18**
2. Expand **Databases**
3. Find and click **PetClothingShopDB**
4. Expand **Schemas** → **public** → **Tables**

You'll see all your tables:
- Users
- Products
- Categories
- Orders
- OrderItems
- Cart
- CartItems
- Reviews
- Addresses
- ProductImages

### Step 4: View Data
1. Right-click on any table (e.g., **Users**)
2. Select **View/Edit Data** → **All Rows**
3. You'll see all the data in that table!

### Step 5: Run Queries
1. Right-click on **PetClothingShopDB**
2. Select **Query Tool**
3. Type SQL queries and click **Execute** (F5)

Example queries:
```sql
-- View admin user
SELECT * FROM "Users" WHERE "Role" = 'Admin';

-- View all categories
SELECT * FROM "Categories";

-- Count products
SELECT COUNT(*) FROM "Products";
```

---

## Method 2: Using Command Line (psql)

### Quick Access:
```powershell
# Set password environment variable
$env:PGPASSWORD = "ramjankh08"

# Connect to database
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d PetClothingShopDB
```

### Common Commands:
```sql
-- List all tables
\dt

-- View Users table
SELECT * FROM "Users";

-- View Categories
SELECT * FROM "Categories";

-- Describe table structure
\d "Users"

-- Exit
\q
```

---

## Method 3: Using PowerShell Script

Save this as `view-database.ps1`:

```powershell
$env:PGPASSWORD = "ramjankh08"
$psql = "C:\Program Files\PostgreSQL\18\bin\psql.exe"

Write-Host "=== Pet Clothing Shop Database Viewer ===" -ForegroundColor Cyan
Write-Host ""

# View Users
Write-Host "USERS:" -ForegroundColor Yellow
& $psql -U postgres -d PetClothingShopDB -c "SELECT ""Id"", ""Email"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"" FROM ""Users"";"

Write-Host ""
Write-Host "CATEGORIES:" -ForegroundColor Yellow
& $psql -U postgres -d PetClothingShopDB -c "SELECT * FROM ""Categories"";"

Write-Host ""
Write-Host "PRODUCTS COUNT:" -ForegroundColor Yellow
& $psql -U postgres -d PetClothingShopDB -c "SELECT COUNT(*) as total FROM ""Products"";"
```

---

## Quick Database Checks

### Check Admin User Exists:
```powershell
$env:PGPASSWORD = "ramjankh08"
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d PetClothingShopDB -c "SELECT ""Email"", ""Role"" FROM ""Users"" WHERE ""Email"" = 'admin@petshop.com';"
```

### View All Tables:
```powershell
$env:PGPASSWORD = "ramjankh08"
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d PetClothingShopDB -c "\dt"
```

---

## Troubleshooting "Failed to Load"

If frontend shows "Failed to load featured products":

### 1. Check Backend is Running
```powershell
Test-NetConnection -ComputerName localhost -Port 5000
```
Should return: `True`

### 2. Test API Directly
Open browser to: http://localhost:5000/api/products/featured?count=5

Should see:
```json
{"success":true,"data":[]}
```

### 3. Check Frontend Environment
Make sure `frontend/.env` has:
```
VITE_API_URL=http://localhost:5000/api
```

### 4. Check Browser Console
1. Open browser to http://localhost:3000
2. Press F12
3. Go to Console tab
4. Look for errors (usually CORS or connection errors)

### 5. Verify Database Connection
```powershell
cd backend\PetClothingShop.API
dotnet run
```
Look for: `Now listening on: http://localhost:5000`

---

## Database Connection String
Location: `backend\PetClothingShop.API\appsettings.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=PetClothingShopDB;Username=postgres;Password=ramjankh08"
}
```

---

## Common SQL Queries

### View Admin User:
```sql
SELECT "Id", "Email", "FirstName", "LastName", "Role", "IsActive", "CreatedAt"
FROM "Users"
WHERE "Email" = 'admin@petshop.com';
```

### View All Categories:
```sql
SELECT "Id", "Name", "Description", "IsActive", "DisplayOrder"
FROM "Categories"
ORDER BY "DisplayOrder";
```

### Count Records:
```sql
SELECT 
    (SELECT COUNT(*) FROM "Users") as total_users,
    (SELECT COUNT(*) FROM "Categories") as total_categories,
    (SELECT COUNT(*) FROM "Products") as total_products;
```

### Add Sample Product (After categories exist):
```sql
INSERT INTO "Products" (
    "Name", "Description", "Price", "SKU", "StockQuantity",
    "CategoryId", "PetType", "Size", "Color", "Material",
    "IsActive", "IsFeatured", "Rating", "ReviewCount", "CreatedAt"
)
VALUES (
    'Cute Dog Sweater',
    'Warm and cozy sweater for small dogs',
    29.99,
    'DOG-SW-001',
    50,
    1,  -- Dog Clothing category
    'Dog',
    'S',
    'Red',
    'Cotton',
    true,
    true,
    4.5,
    0,
    NOW()
);

-- Add product image
INSERT INTO "ProductImages" ("ProductId", "ImageUrl", "IsPrimary", "DisplayOrder", "CreatedAt")
VALUES (
    1,  -- Product ID from above
    'https://via.placeholder.com/400x400/FF6B6B/FFFFFF?text=Dog+Sweater',
    true,
    1,
    NOW()
);
```

---

## Need Help?

1. **Can't connect to database?**
   - Verify PostgreSQL service is running
   - Check password is correct
   - Use pgAdmin 4 (easiest method)

2. **Frontend shows errors?**
   - Check browser console (F12)
   - Verify backend is running (http://localhost:5000)
   - Check CORS settings

3. **Backend won't start?**
   - Check if port 5000 is already in use
   - Verify database connection string
   - Check PostgreSQL is running
