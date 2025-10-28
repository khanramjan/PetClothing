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
    private const string StripeWebhookSecret = "whsec_"; // Will be loaded from config

    public PaymentsController(
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
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
    /// Gets details for a specific payment
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
}
