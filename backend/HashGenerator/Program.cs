using System;
using BCrypt.Net;

var password = "Admin@123";
var hash = BCrypt.Net.BCrypt.HashPassword(password);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"BCrypt Hash: {hash}");
Console.WriteLine();
Console.WriteLine("Verification Test:");
var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
Console.WriteLine($"Password verifies: {isValid}");
