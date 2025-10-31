using Npgsql;

Console.WriteLine("=== Admin Role Updater ===\n");

// Read connection string from environment to avoid hardcoding secrets in repo.
// Supports either a full Npgsql-style connection string in
// ConnectionStrings__DefaultConnection or a DATABASE_URL in the
// postgres://user:pass@host:port/dbname format (common on Supabase/Railway).
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("✗ No database connection string found. Set ConnectionStrings__DefaultConnection or DATABASE_URL environment variable.");
    return 1;
}

// If DATABASE_URL (URI) format is provided, convert to Npgsql connection string
if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var host = uri.Host;
        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var user = userInfo.Length > 0 ? userInfo[0] : string.Empty;
        var pass = userInfo.Length > 1 ? userInfo[1] : string.Empty;
        var db = uri.AbsolutePath.TrimStart('/');

        // Build a plain Npgsql-compatible connection string. This avoids depending on
        // NpgsqlConnectionStringBuilder at compile time and keeps the code simple.
        connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=true";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Failed to parse DATABASE_URL: {ex.Message}");
        return 1;
    }
}

try
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✓ Connected to database\n");

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
