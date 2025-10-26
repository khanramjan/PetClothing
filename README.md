# 🐾 Pet Clothing Shop - Complete E-Commerce Platform

A modern, production-ready, full-stack e-commerce platform for pet clothing built with .NET Core 8, React 18, TypeScript, Tailwind CSS, and PostgreSQL.

## 🌟 Features

### Customer Features
- 🛍️ Browse products with advanced filtering (category, pet type, size, price)
- 🔍 Search functionality
- ⭐ Product ratings and reviews
- 🛒 Shopping cart management
- 💳 Checkout process
- 📦 Order tracking
- 👤 User profile management
- 📍 Multiple shipping addresses

### Admin Features
- 📊 Comprehensive dashboard with analytics
- 📦 Product management (CRUD operations)
- 🏷️ Category management
- 📋 Order management with status updates
- 👥 Customer management
- 📈 Sales reports and charts
- 💾 Inventory tracking

### Security Features
- 🔐 JWT authentication with refresh tokens
- 🔒 Password hashing with BCrypt
- 🛡️ Role-based authorization (Admin, Customer)
- 🚦 Rate limiting
- 🔒 CORS configuration
- ✅ Input validation
- 🛡️ SQL injection prevention
- 🔐 XSS protection

## 🏗️ Architecture

### Backend (.NET Core 8)
- **Clean Architecture** (API, Core, Infrastructure layers)
- **Entity Framework Core** with PostgreSQL
- **Repository Pattern** for data access
- **Service Layer** for business logic
- **JWT Authentication**
- **Swagger/OpenAPI** documentation
- **Serilog** for logging
- **Rate Limiting** with AspNetCoreRateLimit

### Frontend (React + TypeScript)
- **Vite** for fast development
- **TypeScript** for type safety
- **Tailwind CSS** for styling
- **Zustand** for state management
- **React Router v6** for routing
- **React Hook Form** for forms
- **Axios** for API calls
- **React Toastify** for notifications

## 📋 Prerequisites

### Backend
- .NET 8.0 SDK
- PostgreSQL 14+
- Visual Studio 2022 or VS Code

### Frontend
- Node.js 18+
- npm or yarn

## 🚀 Getting Started

### 1. Clone the Repository

```powershell
cd C:\Users\DELL\Desktop\Projects\Pet-Clothing-Shop
```

### 2. Database Setup

#### Install PostgreSQL
Download and install from: https://www.postgresql.org/download/windows/

#### Create Database

```sql
CREATE DATABASE PetClothingShopDB;
```

#### Update Connection String

Edit `backend/PetClothingShop.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=PetClothingShopDB;Username=postgres;Password=YOUR_PASSWORD"
}
```

### 3. Backend Setup

```powershell
# Navigate to API project
cd backend\PetClothingShop.API

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef migrations add InitialCreate --project ..\PetClothingShop.Infrastructure\PetClothingShop.Infrastructure.csproj
dotnet ef database update

# Run the API
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

### 4. Frontend Setup

```powershell
# Navigate to frontend
cd ..\..\frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at: http://localhost:3000

## 📁 Project Structure

```
Pet-Clothing-Shop/
├── backend/
│   ├── PetClothingShop.API/           # Web API project
│   │   ├── Controllers/                # API endpoints
│   │   ├── Program.cs                  # App configuration
│   │   └── appsettings.json           # Configuration
│   ├── PetClothingShop.Core/          # Domain layer
│   │   ├── Entities/                   # Domain models
│   │   ├── DTOs/                       # Data transfer objects
│   │   └── Interfaces/                 # Service contracts
│   └── PetClothingShop.Infrastructure/ # Data access layer
│       ├── Data/                       # DbContext
│       ├── Repositories/               # Data repositories
│       └── Services/                   # Business logic
├── frontend/
│   ├── src/
│   │   ├── components/                 # React components
│   │   ├── pages/                      # Page components
│   │   ├── store/                      # State management
│   │   ├── lib/                        # Utilities
│   │   └── types/                      # TypeScript types
│   ├── public/                         # Static assets
│   └── package.json
└── README.md
```

## 🔧 Configuration

### Backend Configuration

#### JWT Settings (`appsettings.json`)
```json
"Jwt": {
  "Secret": "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS",
  "Issuer": "PetClothingShop",
  "Audience": "PetClothingShopClient",
  "AccessTokenExpiryMinutes": "60"
}
```

#### Rate Limiting
```json
"IpRateLimiting": {
  "GeneralRules": [
    {
      "Endpoint": "*",
      "Period": "1m",
      "Limit": 60
    }
  ]
}
```

### Frontend Configuration

#### Environment Variables (`.env`)
```
VITE_API_URL=http://localhost:5000/api
```

## 📝 API Documentation

Once the backend is running, visit:
- Swagger UI: https://localhost:5001/swagger

### Key Endpoints

#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh-token` - Refresh access token

#### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product details
- `POST /api/products` - Create product (Admin)
- `PUT /api/products/{id}` - Update product (Admin)
- `DELETE /api/products/{id}` - Delete product (Admin)

#### Cart
- `GET /api/cart` - Get user cart
- `POST /api/cart/add` - Add to cart
- `PUT /api/cart/update` - Update cart item
- `DELETE /api/cart/{id}` - Remove from cart

## 👤 Default Users

### Create Admin User

After running migrations, execute this SQL to create an admin:

```sql
-- First, hash a password using BCrypt (use an online tool or .NET code)
-- Example: password "Admin@123" hashed

INSERT INTO "Users" ("Email", "PasswordHash", "FirstName", "LastName", "Role", "IsActive", "CreatedAt")
VALUES (
  'admin@petshop.com',
  '$2a$11$YourHashedPasswordHere',
  'Admin',
  'User',
  'Admin',
  true,
  NOW()
);
```

Or use the backend to register and then manually update the role in the database:

```sql
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'youremail@example.com';
```

## 🧪 Testing

### Backend
```powershell
cd backend\PetClothingShop.API
dotnet test
```

### Frontend
```powershell
cd frontend
npm run test
```

## 📦 Building for Production

### Backend
```powershell
cd backend\PetClothingShop.API
dotnet publish -c Release -o ./publish
```

### Frontend
```powershell
cd frontend
npm run build
```

## 🚀 Deployment

### Backend Deployment (Azure/AWS/Docker)

#### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY ./publish /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "PetClothingShop.API.dll"]
```

### Frontend Deployment

#### Vercel
```powershell
cd frontend
npm install -g vercel
vercel
```

#### Netlify
```powershell
cd frontend
npm install -g netlify-cli
netlify deploy
```

## 🔒 Security Checklist

- ✅ Strong JWT secret (32+ characters)
- ✅ HTTPS enabled
- ✅ Rate limiting configured
- ✅ CORS properly configured
- ✅ SQL injection prevention (EF Core parameterization)
- ✅ XSS protection (React escaping)
- ✅ Password hashing (BCrypt)
- ✅ Input validation
- ✅ Authentication & Authorization

## 🛠️ Technologies Used

### Backend
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- PostgreSQL
- Npgsql
- BCrypt.Net
- JWT (JSON Web Tokens)
- Serilog
- Swagger/OpenAPI
- AspNetCoreRateLimit

### Frontend
- React 18
- TypeScript
- Vite
- Tailwind CSS
- Zustand
- React Router v6
- Axios
- React Hook Form
- React Toastify
- React Icons

## 📧 Support

For issues or questions:
- Create an issue in the repository
- Email: support@petstyle.com

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Icons from React Icons
- UI inspiration from modern e-commerce platforms
- Community feedback and contributions

---

**Built with ❤️ for pet lovers everywhere! 🐕🐈🦜**
