using System;
using BCrypt.Net;

var password = "Admin@123";
var hash = BCrypt.Net.BCrypt.HashPassword(password);

Console.WriteLine("=================================================");
Console.WriteLine("ADMIN USER CREDENTIALS");
Console.WriteLine("=================================================");
Console.WriteLine();
Console.WriteLine($"Email:    admin@petshop.com");
Console.WriteLine($"Password: {password}");
Console.WriteLine();
Console.WriteLine("=================================================");
Console.WriteLine("BCrypt Hashed Password:");
Console.WriteLine("=================================================");
Console.WriteLine(hash);
Console.WriteLine();
Console.WriteLine("=================================================");
Console.WriteLine("SQL INSERT Statement (Copy and run in Railway):");
Console.WriteLine("=================================================");
Console.WriteLine();

var sql = $@"INSERT INTO ""Users"" (
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
    '{hash}',
    'Admin',
    'User',
    '+8801700000000',
    'Admin',
    true,
    NOW()
);";

Console.WriteLine(sql);
Console.WriteLine();
Console.WriteLine("=================================================");
Console.WriteLine("INSTRUCTIONS:");
Console.WriteLine("=================================================");
Console.WriteLine("1. Go to Railway dashboard");
Console.WriteLine("2. Click on PostgreSQL service");
Console.WriteLine("3. Click 'Query' or 'Data' tab");
Console.WriteLine("4. Paste the SQL INSERT statement above");
Console.WriteLine("5. Execute the query");
Console.WriteLine();
Console.WriteLine("Then login with:");
Console.WriteLine("  Email: admin@petshop.com");
Console.WriteLine("  Password: Admin@123");
Console.WriteLine("=================================================");
