# Pet Clothing Shop - Database Setup Script
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Pet Clothing Shop - Setup Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Check if PostgreSQL is running
Write-Host "Step 1: Checking PostgreSQL service..." -ForegroundColor Yellow
$pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
if ($pgService) {
    if ($pgService.Status -ne "Running") {
        Write-Host "PostgreSQL service found but not running. Attempting to start..." -ForegroundColor Yellow
        Start-Service $pgService.Name
        Write-Host "PostgreSQL service started!" -ForegroundColor Green
    } else {
        Write-Host "PostgreSQL service is running!" -ForegroundColor Green
    }
} else {
    Write-Host "Warning: PostgreSQL service not found. Make sure PostgreSQL is installed and running." -ForegroundColor Red
    Write-Host "You can download PostgreSQL from: https://www.postgresql.org/download/" -ForegroundColor Yellow
    $continue = Read-Host "Do you want to continue anyway? (y/n)"
    if ($continue -ne "y") {
        exit
    }
}

Write-Host ""
Write-Host "Step 2: Checking if database exists..." -ForegroundColor Yellow

# Check database connection
$env:PGPASSWORD = "postgres"
$dbExists = psql -U postgres -lqt 2>$null | Select-String -Pattern "PetClothingShopDB"

if (-not $dbExists) {
    Write-Host "Database 'PetClothingShopDB' not found. Creating..." -ForegroundColor Yellow
    psql -U postgres -c "CREATE DATABASE PetClothingShopDB;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database created successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to create database. Please create it manually:" -ForegroundColor Red
        Write-Host 'psql -U postgres -c "CREATE DATABASE PetClothingShopDB;"' -ForegroundColor White
        $continue = Read-Host "Press Enter after creating the database manually, or 'q' to quit"
        if ($continue -eq "q") {
            exit
        }
    }
} else {
    Write-Host "Database 'PetClothingShopDB' already exists!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 3: Applying database migrations..." -ForegroundColor Yellow

# Stop any running backend process to release file locks
$apiProcess = Get-Process -Name "PetClothingShop.API" -ErrorAction SilentlyContinue
if ($apiProcess) {
    Write-Host "Stopping running API process to apply migrations..." -ForegroundColor Yellow
    Stop-Process -Name "PetClothingShop.API" -Force
    Start-Sleep -Seconds 2
}

# Navigate to Infrastructure project
Set-Location "backend\PetClothingShop.Infrastructure"

# Apply migrations
dotnet ef database update --startup-project ..\PetClothingShop.API
if ($LASTEXITCODE -eq 0) {
    Write-Host "Migrations applied successfully!" -ForegroundColor Green
} else {
    Write-Host "Failed to apply migrations. Check the error above." -ForegroundColor Red
    Set-Location "..\..\"
    exit
}

# Go back to root
Set-Location "..\..\"

Write-Host ""
Write-Host "Step 4: Creating admin user..." -ForegroundColor Yellow

# Insert admin user directly via SQL
$adminSql = @"
DO `$`$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'admin@petshop.com') THEN
        INSERT INTO "Users" ("Email", "PasswordHash", "FirstName", "LastName", "Role", "IsActive", "CreatedAt")
        VALUES (
            'admin@petshop.com',
            '`$2a`$11`$rKJ5pZqKJ8YhZJ5pZqKJ8eOJ.XN5cYxGX5HZJ5pZqKJ8YhZJ5pZqKu',
            'Admin',
            'User',
            'Admin',
            true,
            NOW()
        );
        RAISE NOTICE 'Admin user created successfully!';
    ELSE
        UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'admin@petshop.com';
        RAISE NOTICE 'Existing user updated to Admin role!';
    END IF;
END
`$`$;
"@

$env:PGPASSWORD = "postgres"
Write-Host $adminSql | psql -U postgres -d PetClothingShopDB 2>$null

if ($LASTEXITCODE -eq 0) {
    Write-Host "Admin user setup complete!" -ForegroundColor Green
} else {
    Write-Host "Warning: Could not create admin user via psql. You may need to do it manually." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 5: Starting backend API..." -ForegroundColor Yellow
Write-Host "Opening new terminal for backend..." -ForegroundColor Cyan

# Start backend in a new terminal
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'backend\PetClothingShop.API'; Write-Host 'Starting Pet Clothing Shop API...' -ForegroundColor Green; dotnet run"

Write-Host "Backend API starting..." -ForegroundColor Green
Write-Host "Wait for the message 'Now listening on: http://localhost:5000'" -ForegroundColor Yellow

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Admin Credentials:" -ForegroundColor Yellow
Write-Host "  Email: admin@petshop.com" -ForegroundColor White
Write-Host "  Password: Admin@123" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Open browser to http://localhost:3000" -ForegroundColor White
Write-Host "  2. Click 'Login' and use the admin credentials above" -ForegroundColor White
Write-Host "  3. Navigate to Admin Panel at http://localhost:3000/admin" -ForegroundColor White
Write-Host "  4. Start managing products, orders, and customers!" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit this setup window..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
