using Microsoft.EntityFrameworkCore;
using PetClothingShop.Core.Entities;
using PetClothingShop.Infrastructure.Data;

Console.WriteLine("Creating admin user in Railway database...");

var connectionString = "Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Prefer";

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(connectionString);

var context = new AppDbContext(optionsBuilder.Options);

try
{
    // Check if admin exists
    var adminExists = await context.Users.AnyAsync(u => u.Email == "admin@petshop.com");

    if (adminExists)
    {
        Console.WriteLine("⚠ Admin user already exists!");
        Console.WriteLine("Email: admin@petshop.com");
        Console.WriteLine("Password: Admin@123");
    }
    else
    {
        // Create admin user
        var admin = new User
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

        Console.WriteLine("================================================");
        Console.WriteLine("✓ Admin user created successfully!");
        Console.WriteLine("================================================");
        Console.WriteLine();
        Console.WriteLine("You can now login with:");
        Console.WriteLine("  Email:    admin@petshop.com");
        Console.WriteLine("  Password: Admin@123");
        Console.WriteLine();
        Console.WriteLine("================================================");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error: {ex.Message}");
}
finally
{
    await context.DisposeAsync();
}
