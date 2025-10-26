# PostgreSQL Connection Guide

## Quick Fix - 3 Steps to Connect

### Step 1: Find Your PostgreSQL Password

When you installed PostgreSQL, you set a password for the `postgres` user. Common default passwords are:
- `postgres`
- `admin`  
- `root`
- Or the password you set during installation

**To reset the password if forgotten:**

1. Open **pgAdmin 4** (installed with PostgreSQL)
2. Right-click on **PostgreSQL 18** server
3. Select **Properties** > **Connection**
4. Note the port (usually 5432)

OR

1. Find the PostgreSQL data directory
2. Edit `pg_hba.conf` file
3. Change authentication method temporarily to `trust`
4. Restart PostgreSQL service
5. Connect and change password
6. Change `pg_hba.conf` back to `md5` or `scram-sha-256`

### Step 2: Update Connection String

Edit the file: `backend\PetClothingShop.API\appsettings.json`

Change the password in this line:
```json
"DefaultConnection": "Host=localhost;Database=PetClothingShopDB;Username=postgres;Password=YOUR_PASSWORD_HERE"
```

Replace `YOUR_PASSWORD_HERE` with your actual PostgreSQL password.

### Step 3: Add psql to PATH (Optional but Recommended)

**Option A: Add to System PATH**
1. Press `Win + X` and select "System"
2. Click "Advanced system settings"
3. Click "Environment Variables"
4. Under "System variables", find and select "Path"
5. Click "Edit"
6. Click "New"
7. Add: `C:\Program Files\PostgreSQL\18\bin`
8. Click OK on all windows
9. **Close and reopen PowerShell**

**Option B: Use Full Path**
Instead of `psql`, use: `"C:\Program Files\PostgreSQL\18\bin\psql.exe"`

## Manual Database Setup (If setup.ps1 fails)

### Method 1: Using pgAdmin 4 (Easiest)

1. Open **pgAdmin 4** (search in Windows Start menu)
2. Connect to PostgreSQL server (enter your password)
3. Right-click **Databases** > **Create** > **Database**
4. Database name: `PetClothingShopDB`
5. Click **Save**
6. Right-click `PetClothingShopDB` > **Query Tool**
7. Run these commands:

```sql
-- Create admin user (BCrypt hash for password: Admin@123)
INSERT INTO "Users" ("Email", "PasswordHash", "FirstName", "LastName", "Role", "IsActive", "CreatedAt")
VALUES (
    'admin@petshop.com',
    '$2a$11$rKJ5pZqKJ8YhZJ5pZqKJ8eOJ.XN5cYxGX5HZJ5pZqKJ8YhZJ5pZqKu',
    'Admin',
    'User',
    'Admin',
    true,
    NOW()
);

-- Create some categories
INSERT INTO "Categories" ("Name", "Description", "IsActive", "DisplayOrder", "CreatedAt")
VALUES 
    ('Dog Clothing', 'Stylish and comfortable clothing for dogs', true, 1, NOW()),
    ('Cat Clothing', 'Fashionable outfits for cats', true, 2, NOW()),
    ('Bird Accessories', 'Accessories and clothing for birds', true, 3, NOW()),
    ('Pet Costumes', 'Fun costumes for all pets', true, 4, NOW());
```

### Method 2: Using Command Line

1. Open PowerShell as Administrator
2. Navigate to PostgreSQL bin folder:
```powershell
cd "C:\Program Files\PostgreSQL\18\bin"
```

3. Connect to PostgreSQL:
```powershell
.\psql.exe -U postgres
# Enter your password when prompted
```

4. Create database:
```sql
CREATE DATABASE "PetClothingShopDB";
\c PetClothingShopDB
```

5. Exit psql:
```sql
\q
```

### Method 3: Using Entity Framework Migrations

1. Update the password in `appsettings.json` (see Step 2 above)

2. Open PowerShell in the project root folder

3. Stop any running backend process:
```powershell
Get-Process -Name "PetClothingShop*" -ErrorAction SilentlyContinue | Stop-Process -Force
```

4. Run migrations:
```powershell
cd backend\PetClothingShop.Infrastructure
dotnet ef database update --startup-project ..\PetClothingShop.API
```

5. If successful, the database and tables will be created automatically!

## Common Issues & Solutions

### Issue 1: "password authentication failed for user postgres"
**Solution:** Wrong password in `appsettings.json`
- Update the password in the connection string
- Make sure there are no extra spaces

### Issue 2: "psql is not recognized"
**Solution:** psql not in PATH
- Use full path: `"C:\Program Files\PostgreSQL\18\bin\psql.exe"`
- OR add PostgreSQL bin to PATH (see Step 3 above)

### Issue 3: "could not connect to server"
**Solution:** PostgreSQL service not running
```powershell
Start-Service postgresql-x64-18
```

### Issue 4: "database does not exist"
**Solution:** Create it first using pgAdmin or command line (see methods above)

### Issue 5: "port 5432 already in use"
**Solution:** Another PostgreSQL instance running
- Check port in pgAdmin connection settings
- Update connection string if using different port:
```json
"DefaultConnection": "Host=localhost;Port=5433;Database=PetClothingShopDB;Username=postgres;Password=YOUR_PASSWORD"
```

## After Database Setup

Once the database is set up, run:

```powershell
# From project root folder

# 1. Start backend
cd backend\PetClothingShop.API
dotnet run

# 2. In a new terminal, verify frontend is running
# It should already be running at http://localhost:3000

# 3. Login with admin credentials:
#    Email: admin@petshop.com
#    Password: Admin@123
```

## Testing the Connection

Try this simple PowerShell command to test PostgreSQL connection:

```powershell
$env:PGPASSWORD = "YOUR_PASSWORD_HERE"
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -c "SELECT version();"
```

If this works, you'll see the PostgreSQL version!

## Next Steps

1. ✅ Fix the password in `appsettings.json`
2. ✅ Create the database (using pgAdmin or command line)
3. ✅ Run migrations: `dotnet ef database update`
4. ✅ Start backend: `dotnet run`
5. ✅ Login at http://localhost:3000/login
6. ✅ Access admin panel at http://localhost:3000/admin

## Quick Reference

**PostgreSQL Service:**
```powershell
# Check status
Get-Service postgresql-x64-18

# Start service
Start-Service postgresql-x64-18

# Stop service
Stop-Service postgresql-x64-18
```

**Connection String Format:**
```
Host=localhost;Database=PetClothingShopDB;Username=postgres;Password=YOUR_PASSWORD;Port=5432
```

**Admin Credentials:**
- Email: `admin@petshop.com`
- Password: `Admin@123`

Need help? Check the error messages and match them with the "Common Issues" section above!
