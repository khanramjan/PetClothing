# Deploy Backend to Render via CLI

## Prerequisites
- Render account
- GitHub repo pushed
- Render API key

## Quick Deploy

```powershell
# 1. Get Render API key from https://dashboard.render.com/account/api-tokens

# 2. Set variables
$RENDER_API_KEY = "your-api-key"
$GITHUB_REPO = "khanramjan/PetClothing"

# 3. Create service via API
$body = @{
  type = "web_service"
  name = "pet-clothing-shop"
  repo = "https://github.com/$GITHUB_REPO"
  branch = "master"
  buildCommand = "cd backend/PetClothingShop.API && dotnet publish -c Release -o out"
  startCommand = "cd backend/PetClothingShop.API/out && dotnet PetClothingShop.API.dll"
  envVars = @(
    @{ key = "ASPNETCORE_ENVIRONMENT"; value = "Production" }
  )
} | ConvertTo-Json

$headers = @{ Authorization = "Bearer $RENDER_API_KEY" }

Invoke-RestMethod `
  -Uri "https://api.render.com/v1/services" `
  -Method POST `
  -Headers $headers `
  -Body $body `
  -ContentType "application/json"
```

## Easier: Use Web UI
1. Go https://render.com/dashboard
2. New → Web Service
3. Connect GitHub
4. Configure & deploy

Your backend will be live at `https://pet-clothing-shop.onrender.com`

