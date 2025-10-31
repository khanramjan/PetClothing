# Create Admin User in Railway Database
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host " Creating Admin User in Railway PostgreSQL     " -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Railway database connection details
$PGHOST = "mainline.proxy.rlwy.net"
$PGPORT = "47346"
$PGDATABASE = "railway"
$PGUSER = "postgres"
$PGPASSWORD = "PmxjmOySjTjmuUvdSByxMsrmzwExlYli"

# Admin user details
$adminEmail = "admin@petshop.com"
$adminPassword = "Admin@123"
$passwordHash = "$2a`$11`$6MxFkjzGhSlTMMQqZTz2L.9z2052yzsJ4uDrIpUWpgdFUduKXv9L6"

Write-Host "Admin Credentials:" -ForegroundColor Yellow
Write-Host "  Email:    $adminEmail" -ForegroundColor White
Write-Host "  Password: $adminPassword" -ForegroundColor White
Write-Host ""

# Set environment variable for password
$env:PGPASSWORD = $PGPASSWORD

# SQL to insert admin user
$sql = @"
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
    '$adminEmail',
    '$passwordHash',
    'Admin',
    'User',
    '+8801700000000',
    'Admin',
    true,
    NOW()
)
ON CONFLICT (""Email"") DO NOTHING;
"@

Write-Host "Connecting to Railway PostgreSQL..." -ForegroundColor Yellow

# Try to run using psql command
try {
    # Check if psql is installed
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    
    if ($psqlPath) {
        Write-Host "Running SQL to create admin user..." -ForegroundColor Yellow
        
        $result = echo $sql | psql -h $PGHOST -p $PGPORT -U $PGUSER -d $PGDATABASE 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "================================================" -ForegroundColor Green
            Write-Host " Admin User Created Successfully!              " -ForegroundColor Green
            Write-Host "================================================" -ForegroundColor Green
            Write-Host ""
            Write-Host "You can now login with:" -ForegroundColor Cyan
            Write-Host "  Email:    $adminEmail" -ForegroundColor White
            Write-Host "  Password: $adminPassword" -ForegroundColor White
            Write-Host ""
        } else {
            Write-Host ""
            Write-Host "================================================" -ForegroundColor Yellow
            Write-Host " Note: Admin user might already exist         " -ForegroundColor Yellow
            Write-Host "================================================" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Try logging in with:" -ForegroundColor Cyan
            Write-Host "  Email:    $adminEmail" -ForegroundColor White
            Write-Host "  Password: $adminPassword" -ForegroundColor White
            Write-Host ""
        }
    } else {
        Write-Host ""
        Write-Host "================================================" -ForegroundColor Yellow
        Write-Host " psql command not found!                       " -ForegroundColor Yellow
        Write-Host "================================================" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Please install PostgreSQL client tools or:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Manual Method:" -ForegroundColor Cyan
        Write-Host "1. Go to Railway dashboard" -ForegroundColor White
        Write-Host "2. Click on PostgreSQL service" -ForegroundColor White
        Write-Host "3. Click 'Query' tab" -ForegroundColor White
        Write-Host "4. Copy and paste this SQL:" -ForegroundColor White
        Write-Host ""
        Write-Host $sql -ForegroundColor Gray
        Write-Host ""
        Write-Host "5. Click 'Execute' or 'Run Query'" -ForegroundColor White
        Write-Host ""
        Write-Host "Or copy from: ADMIN_CREDENTIALS.md" -ForegroundColor Cyan
        Write-Host ""
    }
} catch {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Red
    Write-Host " Error creating admin user                     " -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please use the manual method:" -ForegroundColor Yellow
    Write-Host "See ADMIN_CREDENTIALS.md for instructions" -ForegroundColor Cyan
    Write-Host ""
}

# Clear password from environment
Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue

Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
