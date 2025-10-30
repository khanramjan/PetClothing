# SSLCommerz Quick Test Script

# Test 1: Verify API is running
Write-Host "`n=== Testing Backend API ===" -ForegroundColor Cyan
try {
    $health = Invoke-WebRequest -Uri "http://localhost:5000/api" -UseBasicParsing -ErrorAction Stop
    Write-Host "✅ Backend is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Backend is not running. Start it with: cd backend/PetClothingShop.API; dotnet run" -ForegroundColor Red
    exit 1
}

# Test 2: Initiate SSLCommerz Payment
Write-Host "`n=== Testing SSLCommerz Payment Initiation ===" -ForegroundColor Cyan

$paymentRequest = @{
    amount = 1000
    currency = "BDT"
    orderId = "TEST-$(Get-Date -Format 'yyyyMMddHHmmss')"
    customerName = "Test Customer"
    customerEmail = "test@example.com"
    customerPhone = "+8801711111111"
    customerCity = "Dhaka"
    customerState = "Dhaka"
    customerPostcode = "1000"
    customerCountry = "Bangladesh"
    description = "Pet Clothing Test Purchase"
} | ConvertTo-Json

Write-Host "Request Body:" -ForegroundColor Yellow
Write-Host $paymentRequest

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/payments/initiate" `
        -Method Post `
        -ContentType "application/json" `
        -Body $paymentRequest

    Write-Host "`n✅ Payment Initiation Successful!" -ForegroundColor Green
    Write-Host "`nResponse:" -ForegroundColor Yellow
    Write-Host ($response | ConvertTo-Json -Depth 5)
    
    if ($response.success -and $response.data.gatewayPageURL) {
        Write-Host "`n🎉 SSLCommerz Integration Working!" -ForegroundColor Green
        Write-Host "Transaction ID: $($response.data.transactionId)" -ForegroundColor Cyan
        Write-Host "`nGateway URL:" -ForegroundColor Yellow
        Write-Host $response.data.gatewayPageURL
        
        Write-Host "`n📋 Next Steps:" -ForegroundColor Cyan
        Write-Host "1. Open the test page: test-sslcommerz.html in a browser"
        Write-Host "2. Or visit the gateway URL directly to complete payment"
        Write-Host "3. Use test card: 4111 1111 1111 1111, CVV: 123, Any future expiry"
        
        # Ask user if they want to open the gateway URL
        $openBrowser = Read-Host "`nOpen payment gateway in browser? (Y/N)"
        if ($openBrowser -eq "Y" -or $openBrowser -eq "y") {
            Start-Process $response.data.gatewayPageURL
        }
    } else {
        Write-Host "❌ Payment initiation failed" -ForegroundColor Red
    }
} catch {
    Write-Host "`n❌ Error during payment initiation" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
