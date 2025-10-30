# Detailed Code Changes for SSLCommerz 404 Fix

## File 1: PaymentDTOs.cs
**Added Two New DTOs for SSLCommerz API Communication**

### Added Classes:
```csharp
/// <summary>
/// SSLCommerz API request to initiate a payment session
/// </summary>
public class SSLCommerzSessionRequest
{
    public string store_id { get; set; } = string.Empty;
    public string store_passwd { get; set; } = string.Empty;
    public string total_amount { get; set; } = string.Empty;
    public string currency { get; set; } = "BDT";
    public string tran_id { get; set; } = string.Empty;
    public string success_url { get; set; } = string.Empty;
    public string fail_url { get; set; } = string.Empty;
    public string cancel_url { get; set; } = string.Empty;
    public string cus_name { get; set; } = string.Empty;
    public string cus_email { get; set; } = string.Empty;
    public string cus_phone { get; set; } = string.Empty;
    public string product_name { get; set; } = string.Empty;
    public string product_category { get; set; } = string.Empty;
    public string product_profile { get; set; } = "general";
}

/// <summary>
/// SSLCommerz API response containing session key
/// </summary>
public class SSLCommerzSessionResponse
{
    public string status { get; set; } = string.Empty;
    public string sessionkey { get; set; } = string.Empty;
    public string gateway_url { get; set; } = string.Empty;
    public string redirectGatewayURL { get; set; } = string.Empty;
    public string errorMessage { get; set; } = string.Empty;
}
```

---

## File 2: IServices.cs (IPaymentService Interface)
**Added New Method Declaration**

### Before:
```csharp
public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(int userId, CreatePaymentIntentRequest request);
    Task<PaymentConfirmationResponse> ConfirmPaymentAsync(int userId, ConfirmPaymentRequest request);
    Task<bool> HandleStripeWebhookAsync(string json, string signature);
    Task<RefundResponse> RefundPaymentAsync(int userId, RefundRequest request);
    Task<PaymentHistoryDTO?> GetPaymentAsync(int paymentId);
    Task<List<PaymentHistoryDTO>> GetUserPaymentHistoryAsync(int userId);
}
```

### After:
```csharp
public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(int userId, CreatePaymentIntentRequest request);
    Task<PaymentConfirmationResponse> ConfirmPaymentAsync(int userId, ConfirmPaymentRequest request);
    Task<bool> HandleStripeWebhookAsync(string json, string signature);
    Task<RefundResponse> RefundPaymentAsync(int userId, RefundRequest request);
    Task<PaymentHistoryDTO?> GetPaymentAsync(int paymentId);
    Task<List<PaymentHistoryDTO>> GetUserPaymentHistoryAsync(int userId);
    Task<InitiatePaymentResponse> InitiateSSLCommerzPaymentAsync(InitiatePaymentRequest request); // ← NEW
}
```

---

## File 3: PaymentService.cs
**Added Imports and New Implementation Methods**

### Added Imports:
```csharp
using System.Net.Http.Json;
using System.Text.Json;
```

### Added Methods:

#### A. Main SSLCommerz Payment Initiation Method
```csharp
/// <summary>
/// Initiates an SSLCommerz payment session by calling their API
/// </summary>
public async Task<InitiatePaymentResponse> InitiateSSLCommerzPaymentAsync(InitiatePaymentRequest request)
{
    try
    {
        _logger.LogInformation($"Initiating SSLCommerz payment for amount: {request.Amount} {request.Currency}");

        // Get SSLCommerz credentials
        var storeId = Environment.GetEnvironmentVariable("SSLCOMMERZ_STORE_ID")
            ?? _configuration["SSLCommerz:StoreId"]
            ?? "testbox";
        var storePassword = Environment.GetEnvironmentVariable("SSLCOMMERZ_STORE_PASSWORD")
            ?? _configuration["SSLCommerz:StorePassword"]
            ?? "qwerty";
        var apiUrl = _configuration["SSLCommerz:ApiUrl"] 
            ?? "https://sandbox.sslcommerz.com/gwprocess/v4/api.php";
        var cashierUrl = _configuration["SSLCommerz:CashierUrl"]
            ?? "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php";

        if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(storePassword))
        {
            _logger.LogError("SSLCommerz credentials not configured");
            throw new InvalidOperationException("SSLCommerz credentials not configured");
        }

        // Create transaction ID
        var transactionId = $"ORDER-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        // Prepare SSLCommerz API request
        var sslRequest = new SSLCommerzSessionRequest
        {
            store_id = storeId,
            store_passwd = storePassword,
            total_amount = request.Amount.ToString("F2"),
            currency = request.Currency,
            tran_id = transactionId,
            success_url = request.SuccessUrl,
            fail_url = request.FailUrl,
            cancel_url = request.CancelUrl,
            cus_name = request.CustomerName,
            cus_email = request.CustomerEmail,
            cus_phone = request.CustomerPhone,
            product_name = request.Description ?? "Pet Clothing",
            product_category = "clothing",
            product_profile = "general"
        };

        // Call SSLCommerz API to create session
        using (var httpClient = new HttpClient())
        {
            var content = new FormUrlEncodedContent(ConvertToKeyValuePairs(sslRequest));
            var response = await httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"SSLCommerz API error: {response.StatusCode} - {error}");
                throw new InvalidOperationException($"SSLCommerz API error: {response.StatusCode}");
            }

            // Parse response - SSLCommerz returns form-encoded or JSON
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"SSLCommerz API response: {responseContent}");

            SSLCommerzSessionResponse sslResponse;
            try
            {
                // Try parsing as JSON first
                sslResponse = JsonSerializer.Deserialize<SSLCommerzSessionResponse>(responseContent)
                    ?? throw new InvalidOperationException("Invalid SSLCommerz response");
            }
            catch
            {
                // If JSON parsing fails, SSLCommerz might return form-encoded or just sessionkey
                // Try to extract sessionkey directly
                if (responseContent.Contains("sessionkey="))
                {
                    var sessionKey = responseContent.Split("sessionkey=")[1].Split("&")[0];
                    sslResponse = new SSLCommerzSessionResponse
                    {
                        status = "SUCCESS",
                        sessionkey = sessionKey,
                        gateway_url = $"{cashierUrl}?sessionkey={Uri.EscapeDataString(sessionKey)}"
                    };
                }
                else
                {
                    throw new InvalidOperationException("Unable to parse SSLCommerz response");
                }
            }

            if (sslResponse.status != "SUCCESS")
            {
                _logger.LogError($"SSLCommerz session creation failed: {sslResponse.errorMessage}");
                throw new InvalidOperationException($"SSLCommerz error: {sslResponse.errorMessage}");
            }

            var gatewayUrl = sslResponse.gateway_url ?? $"{cashierUrl}?sessionkey={Uri.EscapeDataString(sslResponse.sessionkey)}";

            _logger.LogInformation($"SSLCommerz session created successfully. Transaction ID: {transactionId}");

            // Store payment record
            var payment = new Payment
            {
                OrderId = int.TryParse(request.OrderId, out var orderId) ? orderId : 0,
                UserId = 0, // SSLCommerz payments may be from anonymous users
                PaymentIntentId = transactionId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = "pending",
                PaymentMethod = "sslcommerz",
                CreatedAt = DateTime.UtcNow,
            };

            await _paymentRepository.AddAsync(payment);

            return new InitiatePaymentResponse
            {
                TransactionId = transactionId,
                GatewayPageURL = gatewayUrl,
                Message = "Payment gateway URL generated successfully"
            };
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error initiating SSLCommerz payment: {ex.Message}");
        throw;
    }
}
```

#### B. Helper Method to Convert DTO to Key-Value Pairs
```csharp
private Dictionary<string, string> ConvertToKeyValuePairs(SSLCommerzSessionRequest request)
{
    return new Dictionary<string, string>
    {
        { "store_id", request.store_id },
        { "store_passwd", request.store_passwd },
        { "total_amount", request.total_amount },
        { "currency", request.currency },
        { "tran_id", request.tran_id },
        { "success_url", request.success_url },
        { "fail_url", request.fail_url },
        { "cancel_url", request.cancel_url },
        { "cus_name", request.cus_name },
        { "cus_email", request.cus_email },
        { "cus_phone", request.cus_phone },
        { "product_name", request.product_name },
        { "product_category", request.product_category },
        { "product_profile", request.product_profile }
    };
}
```

---

## File 4: PaymentsController.cs
**Updated InitiateSSLCommerz Action Method**

### Before (BROKEN):
```csharp
[HttpPost("initiate")]
[AllowAnonymous]
public async Task<IActionResult> InitiateSSLCommerz([FromBody] InitiatePaymentRequest request)
{
    try
    {
        if (request == null || request.Amount <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid amount" });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = string.IsNullOrEmpty(userIdClaim) ? "0" : userIdClaim;
        _logger.LogInformation($"User {userId} initiating SSLCommerz payment of {request.Amount} {request.Currency}");

        // Get SSLCommerz credentials from environment variables
        var storeId = Environment.GetEnvironmentVariable("SSLCOMMERZ_STORE_ID") 
            ?? _configuration["SSLCommerz:StoreId"] 
            ?? "testbox";
        var storePassword = Environment.GetEnvironmentVariable("SSLCOMMERZ_STORE_PASSWORD") 
            ?? _configuration["SSLCommerz:StorePassword"] 
            ?? "qwerty";
        var isProduction = _configuration.GetValue<bool>("SSLCommerz:IsProduction");
        var cashierUrl = _configuration["SSLCommerz:CashierUrl"] ?? "https://sandbox.sslcommerz.com/gwprocess/v4/cashier.php";

        if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(storePassword))
        {
            _logger.LogError("SSLCommerz credentials not configured");
            return StatusCode(500, new { success = false, message = "Payment gateway not configured" });
        }

        var transactionId = $"{request.OrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

        // ❌ BUILD GATEWAY URL WITH INVALID MANUALLY CONSTRUCTED SESSIONKEY
        var sessionKey = $"{storeId}|{transactionId}|{request.Amount}|{request.Currency}|{userId}";
        var gatewayUrl = $"{cashierUrl}?sessionkey={Uri.EscapeDataString(sessionKey)}";

        _logger.LogInformation($"SSLCommerz payment initiated with transaction ID: {transactionId} for amount: {request.Amount}");

        return Ok(new 
        { 
            success = true, 
            data = new 
            { 
                transactionId,
                gatewayPageURL = gatewayUrl,
                message = "Payment gateway URL generated",
                storeId,
                amount = request.Amount,
                currency = request.Currency
            },
            message = "Payment initiation successful" 
        });
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error initiating SSLCommerz payment: {ex.Message}");
        return StatusCode(500, new { success = false, message = "An error occurred while initiating payment" });
    }
}
```

### After (FIXED):
```csharp
[HttpPost("initiate")]
[AllowAnonymous]
public async Task<IActionResult> InitiateSSLCommerz([FromBody] InitiatePaymentRequest request)
{
    try
    {
        if (request == null || request.Amount <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid amount" });
        }

        if (string.IsNullOrEmpty(request.CustomerEmail) || string.IsNullOrEmpty(request.CustomerName))
        {
            return BadRequest(new { success = false, message = "Customer name and email are required" });
        }

        _logger.LogInformation($"Initiating SSLCommerz payment of {request.Amount} {request.Currency}");

        // ✅ CALL SERVICE METHOD WHICH CALLS SSLCOMMERZ API TO GET VALID SESSIONKEY
        var response = await _paymentService.InitiateSSLCommerzPaymentAsync(request);

        return Ok(new 
        { 
            success = true, 
            data = response,
            message = "Payment gateway URL generated successfully" 
        });
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning($"Invalid operation: {ex.Message}");
        return BadRequest(new { success = false, message = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error initiating SSLCommerz payment: {ex.Message}");
        return StatusCode(500, new { success = false, message = "An error occurred while initiating payment" });
    }
}
```

---

## Summary of Changes

| File | Change Type | Description |
|------|------------|-------------|
| `PaymentDTOs.cs` | Add | Added `SSLCommerzSessionRequest` and `SSLCommerzSessionResponse` DTOs |
| `IServices.cs` | Add | Added `InitiateSSLCommerzPaymentAsync()` method to interface |
| `PaymentService.cs` | Add Imports | Added `System.Net.Http.Json` and `System.Text.Json` |
| `PaymentService.cs` | Add Methods | Added `InitiateSSLCommerzPaymentAsync()` and `ConvertToKeyValuePairs()` |
| `PaymentsController.cs` | Update | Updated `InitiateSSLCommerz()` to call service method |

## Key Difference

**Before**: Manually built sessionkey → SSLCommerz rejects it → 404 error
```csharp
var sessionKey = $"{storeId}|{transactionId}|{request.Amount}|{request.Currency}|{userId}";
```

**After**: Request sessionkey from SSLCommerz API → Use their valid sessionkey → Success
```csharp
var response = await httpClient.PostAsync(apiUrl, content);
var sslResponse = JsonSerializer.Deserialize<SSLCommerzSessionResponse>(responseContent);
var gatewayUrl = $"{cashierUrl}?sessionkey={Uri.EscapeDataString(sslResponse.sessionkey)}";
```

