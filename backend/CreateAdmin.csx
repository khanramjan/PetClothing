using Microsoft.EntityFrameworkCore;
using PetClothingShop.Infrastructure.Data;

var connectionString = "Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Prefer";

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(connectionString);

using var context = new AppDbContext(optionsBuilder.Options);

// Check if admin exists
var adminExists = await context.Users.AnyAsync(u => u.Email == "admin@petshop.com");

if (adminExists)
{
    Console.WriteLine("Admin user already exists!");
}
else
{
    // Create admin user
    var admin = new PetClothingShop.Core.Entities.User
    {
        Email = "admin@petshop.com",
        PasswordHash = "$2a$11$6MxFkjzGhSlTMMQqZTz2L.9z2052yzsJ4uDrIpUWpgdFUduKXv9L6",
        FirstName = "Admin",
        LastName = "User",
        PhoneNumber = "+8801700000000",
        Role = "Admin",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    context.Users.Add(admin);
    await context.SaveChangesAsync();

    Console.WriteLine("✓ Admin user created successfully!");
    Console.WriteLine($"Email: admin@petshop.com");
    Console.WriteLine($"Password: Admin@123");
}
