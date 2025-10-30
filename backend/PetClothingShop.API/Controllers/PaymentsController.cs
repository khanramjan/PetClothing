using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Interfaces;
using System.Security.Claims;

namespace PetClothingShop.API.Controllers;

/// <summary>
/// Handles all payment-related operations (Stripe integration)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;
    private readonly IConfiguration _configuration;
    private const string StripeWebhookSecret = "whsec_"; // Will be loaded from config

    public PaymentsController(
        IPaymentService paymentService,
        ILogger<PaymentsController> logger,
        IConfiguration configuration)
    {
        _paymentService = paymentService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Creates a Stripe Payment Intent for an order
    /// Endpoint: POST /api/payments/create-intent
    /// Returns: PaymentIntentResponse with clientSecret for Stripe Elements
    /// </summary>
    [HttpPost("create-intent")]
    [Authorize]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            if (request == null || request.OrderId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid order ID" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation($"User {userId} requesting payment intent for Order {request.OrderId}");

            var response = await _paymentService.CreatePaymentIntentAsync(userId, request);

            return Ok(new { success = true, data = response, message = "Payment intent created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Invalid operation: {ex.Message}");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating payment intent: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred while creating payment intent" });
        }
    }

    /// <summary>
    /// Confirms a payment after customer provides payment method
    /// Endpoint: POST /api/payments/confirm
    /// Returns: PaymentConfirmationResponse with payment status
    /// </summary>
    [HttpPost("confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.PaymentIntentId))
            {
                return BadRequest(new { success = false, message = "Invalid payment intent" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation($"User {userId} confirming payment for Order {request.OrderId}");

            var response = await _paymentService.ConfirmPaymentAsync(userId, request);

            if (response.Success)
            {
                return Ok(new { success = true, data = response, message = "Payment confirmed successfully" });
            }
            else
            {
                return BadRequest(new { success = false, data = response, message = response.Message });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Invalid operation: {ex.Message}");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error confirming payment: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred while confirming payment" });
        }
    }

    /// <summary>
    /// Webhook endpoint for Stripe events
    /// Endpoint: POST /api/payments/webhook
    /// Handles: payment_intent.succeeded, payment_intent.payment_failed, charge.refunded, etc.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            _logger.LogInformation("Received Stripe webhook");

            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Webhook signature missing");
                return BadRequest("Signature missing");
            }

            var success = await _paymentService.HandleStripeWebhookAsync(json, signature);

            if (success)
            {
                return Ok(new { success = true, message = "Webhook processed" });
            }
            else
            {
                return BadRequest(new { success = false, message = "Webhook processing failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Webhook error: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Webhook processing error" });
        }
    }

    /// <summary>
    /// Requests a refund for a paid order
    /// Endpoint: POST /api/payments/refund
    /// Returns: RefundResponse with refund details
    /// </summary>
    [HttpPost("refund")]
    [Authorize]
    public async Task<IActionResult> RefundPayment([FromBody] RefundRequest request)
    {
        try
        {
            if (request == null || request.OrderId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid order ID" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation($"User {userId} requesting refund for Order {request.OrderId}");

            var response = await _paymentService.RefundPaymentAsync(userId, request);

            return Ok(new { success = response.Success, data = response, message = response.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Invalid operation: {ex.Message}");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing refund: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred while processing refund" });
        }
    }

    /// <summary>
    /// Gets payment history for the current user
    /// Endpoint: GET /api/payments/history
    /// Returns: List of PaymentHistoryDTO
    /// </summary>
    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation($"Retrieving payment history for User {userId}");

            var payments = await _paymentService.GetUserPaymentHistoryAsync(userId);

            return Ok(new { success = true, data = payments, message = "Payment history retrieved" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving payment history: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving payment history" });
        }
    }

    /// <summary>
    /// Initiates an SSLCommerz payment gateway session
    /// Endpoint: POST /api/payments/initiate
    /// Returns: SSLCommerz gateway page URL
    /// </summary>
    [HttpPost("initiate")]
    [Authorize]
    public async Task<IActionResult> InitiateSSLCommerz([FromBody] InitiatePaymentRequest request)
    {
        try
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            _logger.LogInformation($"Payment request received from user {userId}: {System.Text.Json.JsonSerializer.Serialize(request)}");

            if (request == null)
            {
                _logger.LogError("Request is null");
                return BadRequest(new { success = false, message = "Request body is empty" });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid amount" });
            }

            // Use defaults if not provided
            var customerName = request.CustomerName ?? "Guest Customer";
            var customerEmail = request.CustomerEmail ?? "noreply@petclothing.local";
            var customerPhone = request.CustomerPhone ?? "+880";

            _logger.LogInformation($"Initiating SSLCommerz payment of {request.Amount} {request.Currency}");

            // Update request with defaults
            request.CustomerName = customerName;
            request.CustomerEmail = customerEmail;
            request.CustomerPhone = customerPhone;

            var response = await _paymentService.InitiateSSLCommerzPaymentAsync(request, userId);

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

    /// <summary>
    /// Gets the details of a specific payment
    /// Endpoint: GET /api/payments/{paymentId}
    /// Returns: PaymentHistoryDTO
    /// </summary>
    [HttpGet("{paymentId}")]
    [Authorize]
    public async Task<IActionResult> GetPayment(int paymentId)
    {
        try
        {
            var payment = await _paymentService.GetPaymentAsync(paymentId);

            if (payment == null)
            {
                return NotFound(new { success = false, message = "Payment not found" });
            }

            return Ok(new { success = true, data = payment });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving payment: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving payment" });
        }
    }

    /// <summary>
    /// SSLCommerz Success Callback - called when payment succeeds
    /// Endpoint: POST /api/payments/sslcommerz/success
    /// </summary>
    [HttpPost("sslcommerz/success")]
    [AllowAnonymous]
    public async Task<IActionResult> SSLCommerzSuccess([FromForm] SSLCommerzValidationRequest request)
    {
        try
        {
            // Log all form fields received
            var formFields = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            _logger.LogInformation($"SSLCommerz form fields: {System.Text.Json.JsonSerializer.Serialize(formFields)}");
            
            _logger.LogInformation($"SSLCommerz success callback - Transaction: {request.TransactionId}, Status: {request.Status}, tran_id: {request.tran_id}");

            var validationResult = await _paymentService.ValidateSSLCommerzPaymentAsync(request);

            if (validationResult)
            {
                // Redirect to frontend success page
                var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
                return Redirect($"{frontendUrl}/checkout/success?tran_id={request.TransactionId}&amount={request.amount}");
            }
            else
            {
                var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
                return Redirect($"{frontendUrl}/checkout/failed?tran_id={request.TransactionId}&error=validation_failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling SSLCommerz success: {ex.Message}");
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/payment/error");
        }
    }

    /// <summary>
    /// SSLCommerz Fail Callback - called when payment fails
    /// Endpoint: POST /api/payments/sslcommerz/fail
    /// </summary>
    [HttpPost("sslcommerz/fail")]
    [AllowAnonymous]
    public async Task<IActionResult> SSLCommerzFail([FromForm] SSLCommerzValidationRequest request)
    {
        try
        {
            _logger.LogWarning($"SSLCommerz fail callback - Transaction: {request.TransactionId}");

            await _paymentService.HandleSSLCommerzFailureAsync(request);

            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/checkout/failed?tran_id={request.TransactionId}&error=payment_failed");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling SSLCommerz fail: {ex.Message}");
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/payment/error");
        }
    }

    /// <summary>
    /// SSLCommerz Cancel Callback - called when user cancels payment
    /// Endpoint: POST /api/payments/sslcommerz/cancel
    /// </summary>
    [HttpPost("sslcommerz/cancel")]
    [AllowAnonymous]
    public async Task<IActionResult> SSLCommerzCancel([FromForm] SSLCommerzValidationRequest request)
    {
        try
        {
            _logger.LogInformation($"SSLCommerz cancel callback - Transaction: {request.TransactionId}");

            await _paymentService.HandleSSLCommerzCancellationAsync(request);

            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/checkout/failed?tran_id={request.TransactionId}&error=payment_cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling SSLCommerz cancel: {ex.Message}");
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/payment/error");
        }
    }

    /// <summary>
    /// SSLCommerz IPN (Instant Payment Notification) endpoint
    /// Endpoint: POST /api/payments/sslcommerz/ipn
    /// </summary>
    [HttpPost("sslcommerz/ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> SSLCommerzIPN([FromForm] SSLCommerzValidationRequest request)
    {
        try
        {
            _logger.LogInformation($"SSLCommerz IPN received - Transaction: {request.TransactionId}");

            var validationResult = await _paymentService.ValidateSSLCommerzPaymentAsync(request);

            return Ok(new { success = validationResult });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling SSLCommerz IPN: {ex.Message}");
            return StatusCode(500, new { success = false });
        }
    }
}
