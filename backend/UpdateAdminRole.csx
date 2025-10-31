using Npgsql;

Console.WriteLine("================================================");
Console.WriteLine(" Updating admin@petshop.com Role to Admin     ");
Console.WriteLine("================================================");
Console.WriteLine();

var connectionString = "Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Prefer";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    
    Console.WriteLine("Connected to Railway database...");
    
    // Update the role to Admin
    var updateSql = @"UPDATE ""Users"" SET ""Role"" = 'Admin' WHERE ""Email"" = 'admin@petshop.com';";
    
    using var command = new NpgsqlCommand(updateSql, connection);
    var rowsAffected = await command.ExecuteNonQueryAsync();
    
    if (rowsAffected > 0)
    {
        Console.WriteLine();
        Console.WriteLine("================================================");
        Console.WriteLine(" ✓ Successfully updated user role to Admin!    ");
        Console.WriteLine("================================================");
        Console.WriteLine();
        Console.WriteLine("You can now login as admin:");
        Console.WriteLine("  Email:    admin@petshop.com");
        Console.WriteLine("  Password: Admin@123");
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("⚠ User not found. Creating new admin user...");
        Console.WriteLine();
        
        // Create new admin user
        var insertSql = @"
            INSERT INTO ""Users"" (
                ""Email"", 
                ""PasswordHash"", 
                ""FirstName"", 
                ""LastName"", 
                ""PhoneNumber"", 
                ""Role"", 
                ""IsActive"", 
                ""CreatedAt""
            ) VALUES (
                'admin@petshop.com',
                '$2a$11$6MxFkjzGhSlTMMQqZTz2L.9z2052yzsJ4uDrIpUWpgdFUduKXv9L6',
                'Admin',
                'User',
                '+8801700000000',
                'Admin',
                true,
                NOW()
            );";
        
        using var insertCommand = new NpgsqlCommand(insertSql, connection);
        await insertCommand.ExecuteNonQueryAsync();
        
        Console.WriteLine("================================================");
        Console.WriteLine(" ✓ Admin user created successfully!            ");
        Console.WriteLine("================================================");
        Console.WriteLine();
        Console.WriteLine("Login credentials:");
        Console.WriteLine("  Email:    admin@petshop.com");
        Console.WriteLine("  Password: Admin@123");
        Console.WriteLine();
    }
    
    Console.WriteLine("================================================");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("================================================");
    Console.WriteLine(" ✗ Error                                       ");
    Console.WriteLine("================================================");
    Console.WriteLine();
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine();
}
