using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using PetClothingShop.Infrastructure.Data;
using PetClothingShop.Infrastructure.Repositories;
using PetClothingShop.Infrastructure.Services;
using Serilog;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Pet Clothing Shop API", 
        Version = "v1",
        Description = "A modern e-commerce API for pet clothing"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Configuration
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IShippingMethodRepository, ShippingMethodRepository>();
builder.Services.AddScoped<ITaxRateRepository, TaxRateRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pet Clothing Shop API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Serve static files for uploaded images
var uploadsPath = builder.Configuration["FileUpload:Path"] ?? "wwwroot/uploads";
var fullUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);
if (!Directory.Exists(fullUploadsPath))
{
    Directory.CreateDirectory(fullUploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(fullUploadsPath),
    RequestPath = "/uploads"
});

app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed data on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedData(context);
}

app.Run();

// Seed data helper
async Task SeedData(ApplicationDbContext context)
{
    try
    {
        // Apply migrations
        await context.Database.MigrateAsync();

        // Check if data already exists
        if (context.Products.Any())
        {
            // If products exist but images don't, seed the images
            if (!context.ProductImages.Any())
            {
                var existingProducts = await context.Products.ToListAsync();
                if (existingProducts.Count >= 6)
                {
                    var existingImages = new List<ProductImage>
                    {
                        new ProductImage { ProductId = existingProducts[0].Id, ImageUrl = "https://placehold.co/400x400?text=Dog+Hoodie&bg=3b82f6&c=fff", AltText = "Blue dog hoodie front view", IsPrimary = true, DisplayOrder = 1 },
                        new ProductImage { ProductId = existingProducts[0].Id, ImageUrl = "https://placehold.co/400x400?text=Dog+Hoodie+2&bg=3b82f6&c=fff", AltText = "Blue dog hoodie side view", IsPrimary = false, DisplayOrder = 2 },
                        new ProductImage { ProductId = existingProducts[1].Id, ImageUrl = "https://placehold.co/400x400?text=Cat+Sweater&bg=ec4899&c=fff", AltText = "Pink cat sweater", IsPrimary = true, DisplayOrder = 1 },
                        new ProductImage { ProductId = existingProducts[2].Id, ImageUrl = "https://placehold.co/400x400?text=Dog+Rain+Jacket&bg=fbbf24&c=fff", AltText = "Yellow dog rain jacket", IsPrimary = true, DisplayOrder = 1 },
                        new ProductImage { ProductId = existingProducts[3].Id, ImageUrl = "https://placehold.co/400x400?text=Bird+Sweater&bg=ef4444&c=fff", AltText = "Red bird sweater", IsPrimary = true, DisplayOrder = 1 },
                        new ProductImage { ProductId = existingProducts[4].Id, ImageUrl = "https://placehold.co/400x400?text=Dog+Costume&bg=10b981&c=fff", AltText = "Green dinosaur costume for dogs", IsPrimary = true, DisplayOrder = 1 },
                        new ProductImage { ProductId = existingProducts[5].Id, ImageUrl = "https://placehold.co/400x400?text=Cat+Collar&bg=000000&c=fff", AltText = "Black cat collar sweater", IsPrimary = true, DisplayOrder = 1 }
                    };
                    await context.ProductImages.AddRangeAsync(existingImages);
                    await context.SaveChangesAsync();
                    Log.Information("Product images seeded successfully");
                }
            }
            return;
        }

        // Create categories
        var categories = new List<Category>
        {
            new Category { Name = "Dog Clothing", Description = "Stylish and comfortable clothing for dogs", IsActive = true, DisplayOrder = 1 },
            new Category { Name = "Cat Clothing", Description = "Fashionable outfits for cats", IsActive = true, DisplayOrder = 2 },
            new Category { Name = "Bird Accessories", Description = "Accessories and clothing for birds", IsActive = true, DisplayOrder = 3 },
            new Category { Name = "Pet Costumes", Description = "Fun costumes for all pets", IsActive = true, DisplayOrder = 4 }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Create products
        var products = new List<Product>
        {
            new Product 
            { 
                Name = "Classic Dog Hoodie", 
                Description = "Comfortable and stylish hoodie for dogs", 
                Price = 29.99m, 
                DiscountPrice = 24.99m, 
                SKU = "DOG-HOODIE-001", 
                StockQuantity = 50, 
                CategoryId = categories[0].Id, 
                PetType = "Dog", 
                Size = "S,M,L,XL", 
                Color = "Blue", 
                Material = "Cotton Blend", 
                IsActive = true, 
                IsFeatured = true, 
                Rating = 4.5m, 
                ReviewCount = 12 
            },
            new Product 
            { 
                Name = "Cat Sweater", 
                Description = "Warm sweater perfect for cold weather", 
                Price = 24.99m, 
                DiscountPrice = 19.99m, 
                SKU = "CAT-SWEATER-001", 
                StockQuantity = 30, 
                CategoryId = categories[1].Id, 
                PetType = "Cat", 
                Size = "XS,S,M,L", 
                Color = "Pink", 
                Material = "Wool Blend", 
                IsActive = true, 
                IsFeatured = true, 
                Rating = 4.8m, 
                ReviewCount = 8 
            },
            new Product 
            { 
                Name = "Dog Rain Jacket", 
                Description = "Waterproof jacket for rainy days", 
                Price = 34.99m, 
                DiscountPrice = null, 
                SKU = "DOG-RAIN-001", 
                StockQuantity = 25, 
                CategoryId = categories[0].Id, 
                PetType = "Dog", 
                Size = "M,L,XL,XXL", 
                Color = "Yellow", 
                Material = "Polyester", 
                IsActive = true, 
                IsFeatured = false, 
                Rating = 4.2m, 
                ReviewCount = 5 
            },
            new Product 
            { 
                Name = "Bird Sweater", 
                Description = "Tiny sweater for small birds", 
                Price = 12.99m, 
                DiscountPrice = 9.99m, 
                SKU = "BIRD-SWEATER-001", 
                StockQuantity = 100, 
                CategoryId = categories[2].Id, 
                PetType = "Bird", 
                Size = "XS", 
                Color = "Red", 
                Material = "Acrylic", 
                IsActive = true, 
                IsFeatured = false, 
                Rating = 4.0m, 
                ReviewCount = 3 
            },
            new Product 
            { 
                Name = "Dog Costume Set", 
                Description = "Fun dinosaur costume for dogs", 
                Price = 39.99m, 
                DiscountPrice = 29.99m, 
                SKU = "DOG-COSTUME-001", 
                StockQuantity = 15, 
                CategoryId = categories[3].Id, 
                PetType = "Dog", 
                Size = "S,M,L", 
                Color = "Green", 
                Material = "Polyester", 
                IsActive = true, 
                IsFeatured = true, 
                Rating = 4.7m, 
                ReviewCount = 20 
            },
            new Product 
            { 
                Name = "Cat Collar Sweater", 
                Description = "Elegant sweater for cats", 
                Price = 18.99m, 
                DiscountPrice = null, 
                SKU = "CAT-COL-SWEATER", 
                StockQuantity = 40, 
                CategoryId = categories[1].Id, 
                PetType = "Cat", 
                Size = "S,M", 
                Color = "Black", 
                Material = "Cotton", 
                IsActive = true, 
                IsFeatured = false, 
                Rating = 4.3m, 
                ReviewCount = 6 
            }
        };
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Create product images
        var images = new List<ProductImage>
        {
            new ProductImage { ProductId = products[0].Id, ImageUrl = "https://placehold.co/400x400?text=Dog+Hoodie&bg=3b82f6&c=fff", AltText = "Blue dog hoodie front view", IsPrimary = true, DisplayOrder = 1 },
            new ProductImage { ProductId = products[0].Id, ImageUrl = "https://placehold.co/400x400?text=Dog+Hoodie+2&bg=3b82f6&c=fff", AltText = "Blue dog hoodie side view", IsPrimary = false, DisplayOrder = 2 },
            new ProductImage { ProductId = products[1].Id, ImageUrl = "https://placehold.co/400x400?text=Cat+Sweater&bg=ec4899&c=fff", AltText = "Pink cat sweater", IsPrimary = true, DisplayOrder = 1 },
            new ProductImage { ProductId = products[2].Id, ImageUrl = "https://placehold.co/400x400?text=Dog+Rain+Jacket&bg=fbbf24&c=fff", AltText = "Yellow dog rain jacket", IsPrimary = true, DisplayOrder = 1 },
            new ProductImage { ProductId = products[3].Id, ImageUrl = "https://placehold.co/400x400?text=Bird+Sweater&bg=ef4444&c=fff", AltText = "Red bird sweater", IsPrimary = true, DisplayOrder = 1 },
            new ProductImage { ProductId = products[4].Id, ImageUrl = "https://placehold.co/400x400?text=Dog+Costume&bg=10b981&c=fff", AltText = "Green dinosaur costume for dogs", IsPrimary = true, DisplayOrder = 1 },
            new ProductImage { ProductId = products[5].Id, ImageUrl = "https://placehold.co/400x400?text=Cat+Collar&bg=000000&c=fff", AltText = "Black cat collar sweater", IsPrimary = true, DisplayOrder = 1 }
        };
        await context.ProductImages.AddRangeAsync(images);
        await context.SaveChangesAsync();

        Log.Information("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error seeding database");
    }
}
