using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace PetClothingShop.Infrastructure.Services;

/// <summary>
/// Handles all Stripe payment processing, webhook handling, and payment history tracking
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IOrderRepository orderRepository,
        IRepository<Payment> paymentRepository,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _configuration = configuration;
        _logger = logger;

        // Initialize Stripe API Key
        var stripeKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeKey))
        {
            throw new InvalidOperationException("Stripe SecretKey not configured in appsettings.json");
        }
        StripeConfiguration.ApiKey = stripeKey;
    }

    /// <summary>
    /// Creates a Stripe Payment Intent for an order
    /// </summary>
    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(int userId, CreatePaymentIntentRequest request)
    {
        try
        {
            _logger.LogInformation($"Creating payment intent for Order {request.OrderId}, Amount: {request.Amount}");

            // Verify order exists and belongs to user
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null || order.UserId != userId)
            {
                throw new InvalidOperationException("Order not found or unauthorized");
            }

            // Verify order hasn't been paid already
            if (order.PaymentStatus == "Paid")
            {
                throw new InvalidOperationException("Order has already been paid");
            }

            // Create Stripe Payment Intent
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Convert to cents
                Currency = request.Currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = request.Metadata ?? new Dictionary<string, string>
                {
                    { "OrderId", request.OrderId.ToString() },
                    { "UserId", userId.ToString() }
                },
                ReceiptEmail = request.ReceiptEmail,
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            _logger.LogInformation($"Payment Intent created: {paymentIntent.Id}, Status: {paymentIntent.Status}");

            // Store payment record in database
            var payment = new Payment
            {
                OrderId = request.OrderId,
                UserId = userId,
                PaymentIntentId = paymentIntent.Id,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = paymentIntent.Status,
                PaymentMethod = "stripe",
                CreatedAt = DateTime.UtcNow,
            };

            await _paymentRepository.AddAsync(payment);

            return new PaymentIntentResponse
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                Amount = request.Amount,
                Status = paymentIntent.Status,
                Currency = request.Currency,
                CreatedAt = DateTime.UtcNow,
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe error creating payment intent: {ex.Message}");
            throw new InvalidOperationException($"Payment error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error creating payment intent: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Confirms a payment after customer provides payment method
    /// </summary>
    public async Task<PaymentConfirmationResponse> ConfirmPaymentAsync(int userId, ConfirmPaymentRequest request)
    {
        try
        {
            _logger.LogInformation($"Confirming payment for Order {request.OrderId}");

            // Verify order exists and belongs to user
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null || order.UserId != userId)
            {
                throw new InvalidOperationException("Order not found or unauthorized");
            }

            // Retrieve payment record
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(request.PaymentIntentId);

            if (paymentIntent == null)
            {
                throw new InvalidOperationException("Payment intent not found");
            }

            // Update order with payment information
            order.PaymentStatus = paymentIntent.Status == "succeeded" ? "Paid" : "Pending";
            order.PaymentMethod = "card";
            order.PaymentTransactionId = paymentIntent.Id;

            if (paymentIntent.Status == "succeeded")
            {
                order.Status = "Processing"; // Move to processing after payment
                _logger.LogInformation($"Payment succeeded for Order {request.OrderId}");
            }
            else if (paymentIntent.Status == "requires_action")
            {
                _logger.LogWarning($"Payment requires additional action for Order {request.OrderId}");
            }

            await _orderRepository.UpdateAsync(order);

            // Update payment record in database
            var paymentRecord = await _paymentRepository.GetByIdAsync(paymentIntent.Metadata?.GetValueOrDefault("PaymentId") != null
                ? int.Parse(paymentIntent.Metadata["PaymentId"])
                : 0);

            if (paymentRecord != null)
            {
                paymentRecord.Status = paymentIntent.Status;
                paymentRecord.ProcessedAt = DateTime.UtcNow;
                if (paymentIntent.LastPaymentError != null)
                {
                    paymentRecord.FailureReason = paymentIntent.LastPaymentError.Message;
                    paymentRecord.FailureCode = paymentIntent.LastPaymentError.Code;
                }
                await _paymentRepository.UpdateAsync(paymentRecord);
            }

            return new PaymentConfirmationResponse
            {
                Success = paymentIntent.Status == "succeeded",
                Message = paymentIntent.Status == "succeeded"
                    ? "Payment confirmed successfully"
                    : $"Payment status: {paymentIntent.Status}",
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = (decimal)paymentIntent.Amount / 100,
                OrderId = request.OrderId,
                ProcessedAt = DateTime.UtcNow,
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe error confirming payment: {ex.Message}");
            throw new InvalidOperationException($"Payment confirmation error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error confirming payment: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Handles webhook events from Stripe (payment_intent.succeeded, payment_intent.payment_failed, etc.)
    /// </summary>
    public async Task<bool> HandleStripeWebhookAsync(string json, string signature)
    {
        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogWarning("Stripe WebhookSecret not configured");
                return false;
            }

            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

            _logger.LogInformation($"Processing Stripe webhook event: {stripeEvent.Type}");

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentSucceededAsync(stripeEvent);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentFailedAsync(stripeEvent);
                    break;

                case "charge.refunded":
                    await HandleRefundAsync(stripeEvent);
                    break;

                case "payment_intent.canceled":
                    await HandlePaymentCanceledAsync(stripeEvent);
                    break;

                default:
                    _logger.LogInformation($"Unhandled webhook event type: {stripeEvent.Type}");
                    break;
            }

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe webhook error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected webhook error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Processes refunds for orders
    /// </summary>
    public async Task<RefundResponse> RefundPaymentAsync(int userId, RefundRequest request)
    {
        try
        {
            _logger.LogInformation($"Processing refund for Order {request.OrderId}");

            // Verify order exists and belongs to user
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null || order.UserId != userId)
            {
                throw new InvalidOperationException("Order not found or unauthorized");
            }

            if (order.PaymentStatus != "Paid")
            {
                throw new InvalidOperationException("Order has not been paid");
            }

            if (string.IsNullOrEmpty(order.PaymentTransactionId))
            {
                throw new InvalidOperationException("No payment transaction found");
            }

            // Create Stripe refund
            var refundOptions = new RefundCreateOptions
            {
                Amount = request.Amount.HasValue ? (long)(request.Amount.Value * 100) : null,
                Reason = string.IsNullOrEmpty(request.Reason) ? "requested_by_customer" : request.Reason,
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", request.OrderId.ToString() },
                    { "RefundReason", request.Reason ?? "requested_by_customer" }
                }
            };

            var refundService = new RefundService();
            // Refunds can be created with a charge ID or payment intent ID
            // Here we use the payment intent ID from the order
            Stripe.Refund refund;
            try
            {
                refund = await refundService.CreateAsync(refundOptions);
            }
            catch (StripeException)
            {
                // If the above fails, try with the charge/payment intent directly
                refund = await refundService.CreateAsync(refundOptions);
            }

            _logger.LogInformation($"Refund created: {refund.Id}, Status: {refund.Status}");

            // Update order status
            order.PaymentStatus = "Refunded";
            order.Status = "Cancelled";
            await _orderRepository.UpdateAsync(order);

            // Update payment record
            var payments = await _paymentRepository.GetAllAsync();
            var payment = payments.FirstOrDefault(p => p.OrderId == request.OrderId);

            if (payment != null)
            {
                payment.IsRefunded = true;
                payment.RefundId = refund.Id;
                payment.RefundedAmount = (decimal)refund.Amount / 100;
                payment.RefundedAt = DateTime.UtcNow;
                payment.RefundReason = request.Reason;
                await _paymentRepository.UpdateAsync(payment);
            }

            return new RefundResponse
            {
                Success = refund.Status == "succeeded",
                Message = $"Refund processed: {refund.Status}",
                RefundId = refund.Id,
                Amount = (decimal)(refund.Amount / 100L),
                ProcessedAt = DateTime.UtcNow,
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe refund error: {ex.Message}");
            throw new InvalidOperationException($"Refund error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected refund error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets a specific payment record
    /// </summary>
    public async Task<PaymentHistoryDTO?> GetPaymentAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            return null;

        return MapToPaymentHistoryDTO(payment);
    }

    /// <summary>
    /// Gets all payments for a user
    /// </summary>
    public async Task<List<PaymentHistoryDTO>> GetUserPaymentHistoryAsync(int userId)
    {
        var payments = await _paymentRepository.GetAllAsync();
        return payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(MapToPaymentHistoryDTO)
            .ToList();
    }

    // Private helper methods

    private async Task HandlePaymentSucceededAsync(Stripe.Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent?.Metadata == null || !paymentIntent.Metadata.ContainsKey("OrderId"))
        {
            _logger.LogWarning("Payment succeeded but no order metadata found");
            return;
        }

        if (!int.TryParse(paymentIntent.Metadata["OrderId"], out var orderId))
        {
            _logger.LogWarning("Invalid OrderId in payment metadata");
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order != null)
        {
            order.PaymentStatus = "Paid";
            order.PaymentTransactionId = paymentIntent.Id;
            order.Status = "Processing";
            await _orderRepository.UpdateAsync(order);
            _logger.LogInformation($"Order {orderId} payment status updated to Paid");
        }
    }

    private async Task HandlePaymentFailedAsync(Stripe.Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent?.Metadata == null || !paymentIntent.Metadata.ContainsKey("OrderId"))
        {
            _logger.LogWarning("Payment failed but no order metadata found");
            return;
        }

        if (!int.TryParse(paymentIntent.Metadata["OrderId"], out var orderId))
        {
            _logger.LogWarning("Invalid OrderId in payment metadata");
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order != null)
        {
            order.PaymentStatus = "Failed";
            await _orderRepository.UpdateAsync(order);
            _logger.LogInformation($"Order {orderId} payment failed");
        }
    }

    private async Task HandleRefundAsync(Stripe.Event stripeEvent)
    {
        var charge = stripeEvent.Data.Object as Stripe.Charge;
        if (charge == null)
        {
            _logger.LogWarning("Refund event but charge not found");
            return;
        }

        _logger.LogInformation($"Charge {charge.Id} refunded. Amount: {charge.AmountRefunded}");
    }

    private async Task HandlePaymentCanceledAsync(Stripe.Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent?.Metadata == null || !paymentIntent.Metadata.ContainsKey("OrderId"))
        {
            _logger.LogWarning("Payment canceled but no order metadata found");
            return;
        }

        if (!int.TryParse(paymentIntent.Metadata["OrderId"], out var orderId))
        {
            _logger.LogWarning("Invalid OrderId in payment metadata");
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order != null)
        {
            order.PaymentStatus = "Pending";
            await _orderRepository.UpdateAsync(order);
            _logger.LogInformation($"Order {orderId} payment canceled");
        }
    }

    private PaymentHistoryDTO MapToPaymentHistoryDTO(Payment payment)
    {
        return new PaymentHistoryDTO
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            PaymentIntentId = payment.PaymentIntentId,
            Amount = payment.Amount,
            Status = payment.Status,
            PaymentMethod = payment.PaymentMethod,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt,
            FailureReason = payment.FailureReason,
        };
    }
}
