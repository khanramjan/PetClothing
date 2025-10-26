# Pet Clothing Shop - Backend API

A modern, production-level ASP.NET Core 8.0 Web API for an e-commerce pet clothing shop.

## Features

- **Clean Architecture**: Separation of concerns with Core, Infrastructure, and API layers
- **Secure Authentication**: JWT-based authentication with refresh tokens
- **Authorization**: Role-based access control (Admin, Customer)
- **PostgreSQL Database**: Entity Framework Core with migrations
- **Security Features**:
  - Password hashing with BCrypt
  - JWT token authentication
  - Rate limiting
  - CORS configuration
  - Input validation
- **API Documentation**: Swagger/OpenAPI
- **Logging**: Serilog for structured logging
- **File Upload**: Support for product images

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 14+
- Visual Studio 2022 or VS Code

## Getting Started

### 1. Database Setup

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=PetClothingShopDB;Username=your_username;Password=your_password"
}
```

### 2. Apply Migrations

```powershell
cd PetClothingShop.API
dotnet ef migrations add InitialCreate --project ..\PetClothingShop.Infrastructure\PetClothingShop.Infrastructure.csproj
dotnet ef database update
```

### 3. Run the API

```powershell
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/refresh-token` - Refresh access token

### Products
- `GET /api/products` - Get all products (with filtering)
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/featured` - Get featured products
- `POST /api/products` - Create product (Admin only)
- `PUT /api/products/{id}` - Update product (Admin only)
- `DELETE /api/products/{id}` - Delete product (Admin only)

### Cart
- `GET /api/cart` - Get user's cart
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/update` - Update cart item quantity
- `DELETE /api/cart/{cartItemId}` - Remove item from cart
- `DELETE /api/cart/clear` - Clear cart

## Project Structure

```
PetClothingShop.API/
├── Controllers/         # API Controllers
├── Properties/          # Launch settings
├── Program.cs          # Application entry point
└── appsettings.json    # Configuration

PetClothingShop.Core/
├── DTOs/               # Data Transfer Objects
├── Entities/           # Domain models
└── Interfaces/         # Service/Repository interfaces

PetClothingShop.Infrastructure/
├── Data/               # DbContext
├── Repositories/       # Data access layer
└── Services/           # Business logic
```

## Security Configuration

### JWT Settings
Update in `appsettings.json`:
```json
"Jwt": {
  "Secret": "YourSecretKey32CharactersMinimum",
  "Issuer": "PetClothingShop",
  "Audience": "PetClothingShopClient",
  "AccessTokenExpiryMinutes": "60"
}
```

### Rate Limiting
Configured to prevent abuse:
- 60 requests per minute per IP
- 1000 requests per hour per IP

## Default Admin Account

After running migrations, create an admin account:

```sql
INSERT INTO "Users" ("Email", "PasswordHash", "FirstName", "LastName", "Role", "IsActive", "CreatedAt")
VALUES ('admin@petshop.com', '$2a$11$hashed_password_here', 'Admin', 'User', 'Admin', true, NOW());
```

Password: Use BCrypt to hash your password

## Development

### Add New Migration
```powershell
dotnet ef migrations add MigrationName --project ..\PetClothingShop.Infrastructure\PetClothingShop.Infrastructure.csproj
dotnet ef database update
```

### Remove Last Migration
```powershell
dotnet ef migrations remove --project ..\PetClothingShop.Infrastructure\PetClothingShop.Infrastructure.csproj
```

## Production Deployment

1. Update `appsettings.Production.json` with production settings
2. Set strong JWT secret
3. Configure proper CORS origins
4. Enable HTTPS
5. Set up proper logging
6. Configure file upload to cloud storage (AWS S3, Azure Blob, etc.)

## Technologies Used

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- PostgreSQL with Npgsql
- JWT Authentication
- BCrypt.Net for password hashing
- Serilog for logging
- AspNetCoreRateLimit
- Swagger/OpenAPI
