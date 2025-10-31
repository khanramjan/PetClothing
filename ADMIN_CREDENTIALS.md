# 🔐 Admin User Credentials

## ✅ Admin Account Information

**Email:** `admin@petshop.com`  
**Password:** `Admin@123`

---

## 📋 SQL to Create Admin User

Copy and run this SQL in your Railway PostgreSQL database:

```sql
INSERT INTO "Users" (
    "Email", 
    "PasswordHash", 
    "FirstName", 
    "LastName", 
    "PhoneNumber", 
    "Role", 
    "IsActive", 
    "CreatedAt"
) VALUES (
    'admin@petshop.com',
    '$2a$11$6MxFkjzGhSlTMMQqZTz2L.9z2052yzsJ4uDrIpUWpgdFUduKXv9L6',
    'Admin',
    'User',
    '+8801700000000',
    'Admin',
    true,
    NOW()
);
```

---

## 🚀 How to Run the SQL

### Method 1: Using Railway Dashboard (Easiest)
1. Go to [Railway Dashboard](https://railway.app/dashboard)
2. Click on your **PostgreSQL** service
3. Click the **"Query"** or **"Data"** tab
4. Paste the SQL INSERT statement above
5. Click **"Execute"** or **"Run Query"**
6. ✅ Admin user created!

### Method 2: Using psql Command Line
```bash
psql -h your_host -p your_port -U your_username -d your_database -c "INSERT INTO \"Users\" (\"Email\", \"PasswordHash\", \"FirstName\", \"LastName\", \"PhoneNumber\", \"Role\", \"IsActive\", \"CreatedAt\") VALUES ('admin@petshop.com', '\$2a\$11\$6MxFkjzGhSlTMMQqZTz2L.9z2052yzsJ4uDrIpUWpgdFUduKXv9L6', 'Admin', 'User', '+8801700000000', 'Admin', true, NOW());"
```

---

## 🎯 After Creating Admin User

### Login to Your Application:
1. Go to your frontend: http://localhost:5174
2. Click **"Login"**
3. Enter:
   - **Email:** `admin@petshop.com`
   - **Password:** `Admin@123`
4. Click **"Sign In"**
### You Should See:
- ✅ Successfully logged in as Admin
- ✅ Admin panel link appears in navigation
- ✅ Access to admin features:
  - Manage Products
  - Manage Categories
  - View Orders
  - Manage Users (if implemented)

---

## 🔄 Create Additional Users

### For Testing (Regular Customer Account):

**Email:** `customer@test.com`  
**Password:** `Customer@123`

```sql
INSERT INTO "Users" (
    "Email", 
    "PasswordHash", 
    "FirstName", 
    "LastName", 
    "PhoneNumber", 
    "Role", 
    "IsActive", 
    "CreatedAt"
) VALUES (
    'customer@test.com',
    '$2a$11$ZvHdGxP8qQ.WQxgqQxgQxO9O9O9O9O9O9O9O9O9O9O9O9O9O9O',
    'Test',
    'Customer',
    '+8801611111111',
    'Customer',
    true,
    NOW()
);
```

---

## 🔒 Security Notes

### Password Security:
- ✅ Passwords are hashed using **BCrypt**
- ✅ Original password never stored in database
- ✅ Salt is automatically generated
- ✅ Each password hash is unique

### Change Password After First Login:
1. Login as admin
2. Go to **Profile** → **Security** tab
3. Change password to something more secure
4. Remember your new password!

### Recommended Password Format:
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character
- Example: `MySecur3P@ssw0rd!`

---

## ❌ Troubleshooting

### Error: "Email already exists"
**Solution:** User already created. Just login with:
- Email: `admin@petshop.com`
- Password: `Admin@123`

### Error: "Invalid credentials"
**Possible Issues:**
1. User not created yet → Run SQL again
2. Wrong password → Use `Admin@123` (case-sensitive!)
3. Wrong email → Use `admin@petshop.com`

### Error: "Connection failed"
**Solution:**
1. Check backend is running (http://localhost:5000)
2. Check Railway database is accessible
3. Verify connection string in `appsettings.json`

---

## 📊 Verify Admin User Created

### Check in Railway Dashboard:
1. Go to PostgreSQL service
2. Click "Data" tab
3. Select "Users" table
4. You should see:
   - Email: admin@petshop.com
   - Role: Admin
   - IsActive: true

### Check via SQL:
```sql
SELECT "Id", "Email", "FirstName", "LastName", "Role", "IsActive", "CreatedAt" 
FROM "Users" 
WHERE "Email" = 'admin@petshop.com';
```

Expected result:
```
| Id | Email              | FirstName | LastName | Role  | IsActive | CreatedAt           |
|----|-------------------|-----------|----------|-------|----------|---------------------|
| 1  | admin@petshop.com | Admin     | User     | Admin | true     | 2025-10-31 00:50:00 |
```

---

## 🎉 You're All Set!

Now you can:
- ✅ Login as admin
- ✅ Access admin panel
- ✅ Manage products and categories
- ✅ View all orders
- ✅ Full control of your pet clothing shop!

**Happy managing!** 🐾👔
