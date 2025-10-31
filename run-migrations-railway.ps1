# Run Database Migrations to Railway

# IMPORTANT: The internal URL won't work from your local computer
# You need to use the PUBLIC proxy URL from Railway

# From your screenshot, the public URL is:
# mainline.proxy.rlwy.net:47346

# Set the connection string using the PUBLIC URL
$env:ConnectionStrings__DefaultConnection="Host=mainline.proxy.rlwy.net;Port=47346;Database=railway;Username=postgres;Password=PmxjmOySjTjmuUvdSByxMsrmzwExlYli;SSL Mode=Prefer"

# Navigate to backend folder (adjust path if needed)
Set-Location "C:\Users\DELL\Desktop\Projects\Pet-Clothing-Shop\backend"

# Run the migrations
dotnet ef database update --project PetClothingShop.Infrastructure --startup-project PetClothingShop.API --verbose

Write-Host ""
Write-Host "✅ Migrations completed!" -ForegroundColor Green
Write-Host "Check Railway dashboard to verify tables were created" -ForegroundColor Yellow
