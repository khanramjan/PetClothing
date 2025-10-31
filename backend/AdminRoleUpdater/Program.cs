using Npgsql;

Console.WriteLine("=== Admin Role Updater ===\n");

var connectionString = "Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Require;Trust Server Certificate=true";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✓ Connected to Railway database\n");

    // First check if user exists
    var checkSql = @"SELECT ""Id"", ""Email"", ""Role"" FROM ""Users"" WHERE ""Email"" = @Email";
    using var checkCmd = new NpgsqlCommand(checkSql, connection);
    checkCmd.Parameters.AddWithValue("@Email", "admin@petshop.com");
    
    using var reader = await checkCmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        var userId = reader.GetInt32(0);
        var email = reader.GetString(1);
        var currentRole = reader.GetString(2);
        
        Console.WriteLine($"Found user:");
        Console.WriteLine($"  ID: {userId}");
        Console.WriteLine($"  Email: {email}");
        Console.WriteLine($"  Current Role: {currentRole}\n");
        
        await reader.CloseAsync();
        
        if (currentRole == "Admin")
        {
            Console.WriteLine("✓ User already has Admin role. No update needed.");
        }
        else
        {
            // Update role to Admin
            var updateSql = @"UPDATE ""Users"" SET ""Role"" = @Role WHERE ""Email"" = @Email";
            using var updateCmd = new NpgsqlCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@Role", "Admin");
            updateCmd.Parameters.AddWithValue("@Email", "admin@petshop.com");
            
            var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                Console.WriteLine($"✓ Successfully updated role from '{currentRole}' to 'Admin'");
                Console.WriteLine($"\nAdmin credentials:");
                Console.WriteLine($"  Email: admin@petshop.com");
                Console.WriteLine($"  Password: Admin@123");
            }
            else
            {
                Console.WriteLine("✗ Failed to update role");
            }
        }
    }
    else
    {
        Console.WriteLine("✗ User 'admin@petshop.com' not found in database");
        Console.WriteLine("\nPlease register the user first through the API:");
        Console.WriteLine("  POST http://localhost:5000/api/auth/register");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Error: {ex.Message}");
    Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
    return 1;
}

Console.WriteLine("\n=== Done ===");
return 0;
