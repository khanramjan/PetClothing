# Register admin user via API
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host " Creating Admin User via API                   " -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$apiUrl = "http://localhost:5000/api/auth/register"

$adminData = @{
    email = "admin@petshop.com"
    password = "Admin@123"
    confirmPassword = "Admin@123"
    firstName = "Admin"
    lastName = "User"
    phoneNumber = "+8801700000000"
} | ConvertTo-Json

try {
    Write-Host "Registering admin user..." -ForegroundColor Yellow
    
    $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Body $adminData -ContentType "application/json" -ErrorAction Stop
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host " Admin User Created Successfully!              " -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Login credentials:" -ForegroundColor Cyan
    Write-Host "  Email:    admin@petshop.com" -ForegroundColor White
    Write-Host "  Password: Admin@123" -ForegroundColor White
    Write-Host ""
    
    # Now update the user role to Admin in database
    Write-Host "Updating user role to Admin..." -ForegroundColor Yellow
    
    $updateSql = "UPDATE \`"Users\`" SET \`"Role\`" = 'Admin' WHERE \`"Email\`" = 'admin@petshop.com';"
    
    Write-Host ""
    Write-Host "Please run this SQL in Railway dashboard:" -ForegroundColor Yellow
    Write-Host $updateSql -ForegroundColor Gray
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    
} catch {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Red
    Write-Host " Error creating user                           " -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Yellow
    }
}

Write-Host ""
