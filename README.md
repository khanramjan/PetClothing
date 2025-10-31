# 🛍️ Pet Clothing Shop

A full-stack e-commerce platform for pet clothing built with React, ASP.NET Core, and PostgreSQL.

## 🚀 Features

- 🛒 Shopping cart with real-time updates
- 💳 Multiple payment gateways (SSLCommerz, Stripe)
- 👤 User authentication & profile management
- 📦 Order management system
- 🔐 JWT-based authentication
- 🎨 Modern UI with Tailwind CSS
- 📱 Responsive design

## 🛠️ Tech Stack

### Frontend
- React 18
- TypeScript
- Vite
- Tailwind CSS
- Zustand (State Management)
- Axios

### Backend
- ASP.NET Core 8.0
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- BCrypt Password Hashing

## 📋 Prerequisites

- [Node.js](https://nodejs.org/) (v18 or higher)
- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (or Railway/cloud instance)

## ⚙️ Installation

### 1. Clone the Repository

```bash
git clone https://github.com/khanramjan/PetClothing.git
cd PetClothing
```

### 2. Backend Setup

1. Navigate to the backend directory:
```bash
cd backend/PetClothingShop.API
```

2. Create `appsettings.json` from the example:
```bash
cp appsettings.EXAMPLE.json appsettings.json
```

3. Update `appsettings.json` with your configuration:
   - Database connection string
   - JWT secret key
   - SSLCommerz credentials (sandbox or production)
   - Frontend/Backend URLs

4. Apply database migrations:
```bash
cd ..
dotnet ef database update --project PetClothingShop.Infrastructure --startup-project PetClothingShop.API
```

5. Run the backend:
```bash
cd PetClothingShop.API
dotnet run
```

Backend will start on `http://localhost:5000`

### 3. Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Create `.env.local` from example (if needed):
```bash
cp .env.example .env.local
```

4. Start the development server:
```bash
npm run dev
```

Frontend will start on `http://localhost:5173`

## 🗄️ Database Configuration

### Option 1: Local PostgreSQL

Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=PetClothingShopDB;Username=postgres;Password=your_password"
  }
}
```

### Option 2: Railway (Cloud Database)

1. Create a PostgreSQL database on [Railway](https://railway.app)
2. Get your connection string from Railway dashboard
3. Update `appsettings.json` with Railway credentials:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your_host.proxy.rlwy.net;Port=xxxxx;Database=railway;Username=postgres;Password=xxxxx;SSL Mode=Prefer;Trust Server Certificate=true"
  }
}
```

## 👤 Create Admin User

1. Generate password hash:
```bash
cd backend/HashGenerator
dotnet run
```

2. Update user role in database:
```sql
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'your_admin_email@example.com';
```

Or use the provided console app:
```bash
cd backend/AdminRoleUpdater
dotnet run
```

## 🔐 Environment Variables

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your database connection string"
  },
  "Jwt": {
    "Secret": "Your JWT secret key (min 32 characters)",
    "Issuer": "PetClothingShop",
    "Audience": "PetClothingShopClient"
  },
  "SSLCommerz": {
    "StoreId": "your_store_id",
    "StorePassword": "your_store_password"
  }
}
```

### Frontend (.env.local) - Optional

```env
VITE_API_URL=http://localhost:5000
```

## 🚢 Deployment

### Backend - Railway

1. Create a new project on Railway
2. Add PostgreSQL database service
3. Add your ASP.NET Core app
4. Set environment variable:
   ```
   ConnectionStrings__DefaultConnection=<your_railway_db_connection_string>
   ```
5. Deploy from GitHub

### Frontend - Vercel

1. Import your repository to Vercel
2. Set build command: `npm run build`
3. Set output directory: `dist`
4. Add environment variable:
   ```
   VITE_API_URL=<your_backend_url>
   ```
5. Deploy

## 📚 API Documentation

The backend API runs on `http://localhost:5000` with the following main endpoints:

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh token

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/featured` - Get featured products

### Cart
- `GET /api/cart` - Get user cart
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/update/{id}` - Update cart item
- `DELETE /api/cart/remove/{id}` - Remove from cart

### Orders
- `GET /api/orders` - Get user orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create order

### Payment
- `POST /api/payment/sslcommerz/initiate` - Initiate SSLCommerz payment
- `POST /api/payment/sslcommerz/success` - Payment success callback
- `POST /api/payment/sslcommerz/fail` - Payment failure callback

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

## 📝 License

This project is licensed under the MIT License.

## 👥 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📧 Support

For support, email khanramjan@example.com or create an issue in the repository.

## 🙏 Acknowledgments

- SSLCommerz for payment gateway integration
- Railway for database hosting
- All contributors to this project
