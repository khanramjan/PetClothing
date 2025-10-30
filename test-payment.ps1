$body = @{
    amount = 1000
    currency = "BDT"
    orderId = "TEST123"
    customerName = "Test User"
    customerEmail = "test@example.com"
    customerPhone = "+8801711111111"
    customerCity = "Dhaka"
    customerState = "Dhaka"
    customerPostcode = "1000"
    customerCountry = "Bangladesh"
    description = "Test Payment"
}

$json = $body | ConvertTo-Json
Write-Host "Request:" -ForegroundColor Cyan
Write-Host $json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/payments/initiate" -Method Post -ContentType "application/json" -Body $json
    Write-Host "`nSuccess!" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
    Write-Host "`nGateway URL:" -ForegroundColor Yellow
    Write-Host $response.data.gatewayPageURL
} catch {
    Write-Host "`nError:" -ForegroundColor Red
    Write-Host $_.Exception.Message
}
