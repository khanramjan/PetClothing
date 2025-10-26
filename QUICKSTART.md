# Pet Clothing Shop - Quick Start Guide

## Prerequisites
- PostgreSQL 14 or higher
- .NET 8.0 SDK
- Node.js 18 or higher
- npm or yarn

## Quick Setup (5 minutes)

### 1. Database Setup
```powershell
# Start PostgreSQL service (if not already running)
# Open psql command line or pgAdmin

# Create database
CREATE DATABASE PetClothingShopDB;
```

### 2. Backend Setup
```powershell
# Navigate to API project
cd backend/PetClothingShop.API

# Restore dependencies
dotnet restore

# Run migrations
dotnet ef database update

# Start the API
dotnet run
```

The API will run at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger**: https://localhost:5001/swagger

### 3. Frontend Setup
```powershell
# In a new terminal, navigate to frontend
cd frontend

# Install dependencies (if not already done)
npm install

# Start development server
npm run dev
```

The frontend will run at: **http://localhost:3000**

## Default Admin Account
After running migrations, you'll need to create an admin account:

**Option 1**: Use the register endpoint with role modification
```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "admin@petshop.com",
  "password": "Admin@123",
  "firstName": "Admin",
  "lastName": "User"
}
```

Then manually update the role in database:
```sql
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'admin@petshop.com';
```

**Option 2**: Use pgAdmin to insert admin directly

## Testing the Application

### 1. Access the Application
- Open browser to http://localhost:3000
- You should see the homepage with a hero section

### 2. Create Products (Admin)
- Login with admin credentials
- Navigate to http://localhost:3000/admin/products
- Click "Add Product" button
- Fill in the form with product details
- Add image URLs (you can use placeholder image URLs like `https://via.placeholder.com/400`)

### 3. Test Customer Features
- Logout from admin
- Register a new customer account
- Browse products
- Add items to cart
- Proceed to checkout

## Common Issues

### Issue: Backend won't start
**Solution**: Check if PostgreSQL is running and connection string in `appsettings.json` is correct

### Issue: Frontend shows API errors
**Solution**: Ensure backend is running on port 5000/5001

### Issue: Database migrations fail
**Solution**: 
```powershell
# Delete existing migrations
Remove-Item -Recurse -Force Migrations

# Create new migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Issue: CORS errors
**Solution**: Verify frontend is running on port 3000 (configured in Program.cs)

## Project Structure
```
Pet-Clothing-Shop/
├── backend/
│   ├── PetClothingShop.API/          # Web API Layer
│   ├── PetClothingShop.Core/         # Domain Models & Interfaces
│   └── PetClothingShop.Infrastructure/ # Data Access & Services
└── frontend/
    ├── src/
    │   ├── components/               # Reusable components
    │   ├── pages/                    # Page components
    │   │   ├── admin/               # Admin panel pages
    │   │   └── ...                  # Customer pages
    │   ├── store/                    # Zustand state management
    │   └── lib/                      # API client & utilities
    └── public/                       # Static assets
```

## Key Features Implemented

### Admin Panel
✅ **Dashboard** with analytics and charts
✅ **Product Management** - Full CRUD with image upload support
✅ **Order Management** - View and update order status
✅ **Customer Management** - View customer list
✅ **Category Management** - Organize products
✅ **Review Management** - Approve/reject reviews

### Customer Features
✅ **Product Browsing** with filters and search
✅ **Shopping Cart** with real-time updates
✅ **Checkout Process** with address management
✅ **Order History** and tracking
✅ **Product Reviews** and ratings
✅ **User Profile** management

### Security
✅ JWT Authentication with refresh tokens
✅ Role-based authorization (Admin/Customer)
✅ BCrypt password hashing
✅ Rate limiting (60 req/min, 1000 req/hour)
✅ CORS protection
✅ SQL injection protection (EF Core parameterized queries)

## Next Steps

1. **Add Sample Data**: Create categories and products through admin panel
2. **Customize Design**: Update colors in `tailwind.config.js`
3. **Add Payment Gateway**: Integrate Stripe or PayPal
4. **Email Notifications**: Implement order confirmation emails
5. **File Upload**: Add real image upload (currently uses URLs)
6. **Production Deployment**: Configure for production environment

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh-token` - Refresh access token

### Products
- `GET /api/products` - Get all products (with pagination/filtering)
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/featured` - Get featured products
- `POST /api/products` - Create product (Admin)
- `PUT /api/products/{id}` - Update product (Admin)
- `DELETE /api/products/{id}` - Delete product (Admin)

### Categories
- `GET /api/categories` - Get all categories
- `POST /api/categories` - Create category (Admin)
- `PUT /api/categories/{id}` - Update category (Admin)
- `DELETE /api/categories/{id}` - Delete category (Admin)

### Cart
- `GET /api/cart` - Get user cart
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/update` - Update cart item
- `DELETE /api/cart/{itemId}` - Remove cart item
- `DELETE /api/cart/clear` - Clear cart

### Orders
- `POST /api/orders` - Create order
- `GET /api/orders` - Get user orders
- `GET /api/orders/{id}` - Get order details
- `GET /api/orders/admin/all` - Get all orders (Admin)
- `PUT /api/orders/admin/status` - Update order status (Admin)

### Admin
- `GET /api/admin/dashboard` - Get dashboard statistics

## Support
For issues or questions, check the main README.md or create an issue in the repository.

## License
MIT License - Feel free to use for learning and commercial projects.
