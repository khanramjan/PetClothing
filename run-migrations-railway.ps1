# Run Database Migrations to Supabase PostgreSQL

Write-Host ''
Write-Host '===============================================' -ForegroundColor Cyan
Write-Host ' Running EF Core Migrations to Supabase       ' -ForegroundColor Cyan
Write-Host '===============================================' -ForegroundColor Cyan
Write-Host ''

# Use environment variable for connection string
# Set this BEFORE running this script, or edit the value below
#
# Option 1: Via environment variable (recommended for automation)
#   $env:ConnectionStrings__DefaultConnection = 'Host=your_host;Port=5432;Database=postgres;Username=postgres;Password=your_password;SSL Mode=Require;Trust Server Certificate=true'
#
# Option 2: Via .env file - see SUPABASE_MIGRATION_GUIDE.md

# Check if connection string is set
if (-not $env:ConnectionStrings__DefaultConnection) {
    Write-Host ' Error: ConnectionStrings__DefaultConnection environment variable not set' -ForegroundColor Red
    Write-Host ''
    Write-Host 'Set it before running this script:' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '$env:ConnectionStrings__DefaultConnection = "Host=your_host;Port=5432;Database=postgres;Username=postgres;Password=your_password;SSL Mode=Require;Trust Server Certificate=true"' -ForegroundColor Gray
    Write-Host '.\run-migrations-railway.ps1' -ForegroundColor Gray
    Write-Host ''
    exit 1
}

Write-Host 'Connection string is set ' -ForegroundColor Green
Write-Host ''

# Navigate to backend folder (adjust path if needed)
Set-Location 'C:\Users\DELL\Desktop\Projects\Pet-Clothing-Shop\backend'

Write-Host 'Running migrations...' -ForegroundColor Yellow
Write-Host ''

# Run the migrations
dotnet ef database update `
    --project PetClothingShop.Infrastructure `
    --startup-project PetClothingShop.API `
    --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host ''
    Write-Host '===============================================' -ForegroundColor Green
    Write-Host '  Migrations completed successfully!         ' -ForegroundColor Green
    Write-Host '===============================================' -ForegroundColor Green
    Write-Host ''
    Write-Host 'Verify in Supabase dashboard:' -ForegroundColor Cyan
    Write-Host '  1. Go to https://app.supabase.com' -ForegroundColor White
    Write-Host '  2. Select your project (sdhpxjpbbevqhgiwtltz)' -ForegroundColor White
    Write-Host '  3. Click Table Editor to see all tables' -ForegroundColor White
    Write-Host ''
} else {
    Write-Host ''
    Write-Host '===============================================' -ForegroundColor Red
    Write-Host '  Migrations failed!                         ' -ForegroundColor Red
    Write-Host '===============================================' -ForegroundColor Red
    Write-Host ''
    Write-Host 'Troubleshooting:' -ForegroundColor Yellow
    Write-Host '  1. Check connection string is correct' -ForegroundColor White
    Write-Host '  2. Verify Supabase database is running' -ForegroundColor White
    Write-Host '  3. See SUPABASE_MIGRATION_GUIDE.md for details.' -ForegroundColor White
    Write-Host ''
    exit 1
}
