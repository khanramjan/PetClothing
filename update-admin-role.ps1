# Update user role to Admin
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host " Updating admin@petshop.com Role to Admin     " -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "MANUAL STEP REQUIRED:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Go to Railway dashboard and run this SQL:" -ForegroundColor White
Write-Host ""
Write-Host "UPDATE \"Users\" SET \"Role\" = 'Admin' WHERE \"Email\" = 'admin@petshop.com';" -ForegroundColor Cyan
Write-Host ""
Write-Host "Steps:" -ForegroundColor Yellow
Write-Host "1. Go to Railway dashboard" -ForegroundColor White
Write-Host "2. Click PostgreSQL service" -ForegroundColor White
Write-Host "3. Click 'Query' tab" -ForegroundColor White
Write-Host "4. Paste the SQL above" -ForegroundColor White
Write-Host "5. Click 'Execute'" -ForegroundColor White
Write-Host ""
Write-Host "Then login with:" -ForegroundColor Cyan
Write-Host "  Email:    admin@petshop.com" -ForegroundColor White
Write-Host "  Password: Admin@123" -ForegroundColor White
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
