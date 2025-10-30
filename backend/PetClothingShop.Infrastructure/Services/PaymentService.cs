using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using Stripe;
using Stripe.Checkout;
using System.Net.Http.Json;
using System.Text.Json;

namespace PetClothingShop.Infrastructure.Services;

/// <summary>
/// Handles all Stripe payment processing, webhook handling, and payment history tracking
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Core.Entities.Address> _addressRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IOrderRepository orderRepository,
        IRepository<Payment> paymentRepository,
        ICartRepository cartRepository,
        IRepository<CartItem> cartItemRepository,
        IRepository<User> userRepository,
        IRepository<Core.Entities.Address> addressRepository,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _userRepository = userRepository;
        _addressRepository = addressRepository;
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

    /// <summary>
    /// Initiates an SSLCommerz payment session by calling their API
    /// </summary>
    public async Task<InitiatePaymentResponse> InitiateSSLCommerzPaymentAsync(InitiatePaymentRequest request, int userId)
    {
        try
        {
            _logger.LogInformation($"Initiating SSLCommerz payment for amount: {request.Amount} {request.Currency}");

            // Get SSLCommerz credentials from configuration
            var storeId = _configuration["SSLCommerz:StoreId"];
            var storePassword = _configuration["SSLCommerz:StorePassword"];
            var apiUrl = _configuration["SSLCommerz:ApiUrl"] 
                ?? "https://sandbox.sslcommerz.com/gwprocess/v4/api.php";
            var isProduction = bool.Parse(_configuration["SSLCommerz:IsProduction"] ?? "false");

            if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(storePassword))
            {
                _logger.LogError("SSLCommerz credentials not configured");
                throw new InvalidOperationException("SSLCommerz credentials not configured");
            }

            _logger.LogInformation($"Using SSLCommerz Store ID: {storeId}, Production: {isProduction}");

            // Create transaction ID
            var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Get base URL for callbacks
            var baseUrl = _configuration["Backend:BaseUrl"] ?? "http://localhost:5000";

            // Prepare SSLCommerz API request
            var sslRequest = new SSLCommerzSessionRequest
            {
                store_id = storeId,
                store_passwd = storePassword,
                total_amount = request.Amount.ToString("F2"),
                currency = request.Currency,
                tran_id = transactionId,
                success_url = $"{baseUrl}/api/payments/sslcommerz/success",
                fail_url = $"{baseUrl}/api/payments/sslcommerz/fail",
                cancel_url = $"{baseUrl}/api/payments/sslcommerz/cancel",
                
                // Customer Information
                cus_name = request.CustomerName,
                cus_email = request.CustomerEmail,
                cus_phone = request.CustomerPhone,
                cus_add1 = request.Description ?? "Pet Clothing Purchase",
                cus_city = request.CustomerCity ?? "Dhaka",
                cus_postcode = request.CustomerPostcode ?? "1000",
                cus_state = request.CustomerState ?? "Dhaka",
                cus_country = request.CustomerCountry ?? "Bangladesh",
                
                // Shipping Information
                shipping_method = "YES",
                ship_name = request.CustomerName,
                ship_add1 = request.Description ?? "Pet Clothing Purchase",
                ship_city = request.CustomerCity ?? "Dhaka",
                ship_state = request.CustomerState ?? "Dhaka",
                ship_postcode = request.CustomerPostcode ?? "1000",
                ship_country = request.CustomerCountry ?? "Bangladesh",
                
                // Product Information
                product_name = request.Description ?? "Pet Clothing",
                product_category = "clothing",
                product_profile = "general",
                
                // Custom data - will be returned in callback
                value_d = request.CustomerEmail // Store user email to retrieve cart later
            };

            _logger.LogInformation($"SSLCommerz request - Amount: {sslRequest.total_amount}, TranID: {transactionId}");

            // Call SSLCommerz API to create session
            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(ConvertToKeyValuePairs(sslRequest));
                var response = await httpClient.PostAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"SSLCommerz API Response Status: {response.StatusCode}");
                _logger.LogInformation($"SSLCommerz API Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"SSLCommerz API error: {response.StatusCode} - {responseContent}");
                    throw new InvalidOperationException($"SSLCommerz API error: {response.StatusCode}");
                }

                // Parse response - SSLCommerz returns JSON
                SSLCommerzSessionResponse sslResponse;
                try
                {
                    sslResponse = JsonSerializer.Deserialize<SSLCommerzSessionResponse>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? throw new InvalidOperationException("Invalid SSLCommerz response");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to parse SSLCommerz response: {ex.Message}");
                    throw new InvalidOperationException($"Failed to parse SSLCommerz response: {ex.Message}");
                }

                if (sslResponse.status?.ToUpper() != "SUCCESS")
                {
                    _logger.LogError($"SSLCommerz session creation failed: {sslResponse.errorMessage}");
                    throw new InvalidOperationException($"SSLCommerz error: {sslResponse.errorMessage ?? "Unknown error"}");
                }

                // Use GatewayPageURL for EasyCheckout (shows all payment options: cards, mobile banking, internet banking)
                // redirectGatewayURL is only for direct card payment
                var gatewayUrl = sslResponse.GatewayPageURL ?? sslResponse.redirectGatewayURL ?? sslResponse.gateway_url;

                if (string.IsNullOrEmpty(gatewayUrl))
                {
                    _logger.LogError("SSLCommerz did not return a gateway URL");
                    throw new InvalidOperationException("SSLCommerz did not return a gateway URL");
                }

                _logger.LogInformation($"SSLCommerz session created successfully. Transaction ID: {transactionId}, Gateway URL: {gatewayUrl}");

                // Create a pending payment record with user ID so we can retrieve it in the callback
                var pendingPayment = new Payment
                {
                    UserId = userId,
                    PaymentIntentId = transactionId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Status = "pending",
                    PaymentMethod = "sslcommerz",
                    CreatedAt = DateTime.UtcNow
                };
                await _paymentRepository.AddAsync(pendingPayment);
                _logger.LogInformation($"Pending payment record created for user {userId}, transaction {transactionId}");

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

    /// <summary>
    /// Validates SSLCommerz payment by verifying with their API
    /// </summary>
    public async Task<bool> ValidateSSLCommerzPaymentAsync(SSLCommerzValidationRequest request)
    {
        try
        {
            _logger.LogInformation($"Validating SSLCommerz payment - Transaction: {request.TransactionId}");

            var storeId = _configuration["SSLCommerz:StoreId"];
            var storePassword = _configuration["SSLCommerz:StorePassword"];
            var validationUrl = _configuration["SSLCommerz:ValidationUrl"] 
                ?? "https://sandbox.sslcommerz.com/validator/api/validationserverAPI.php";

            if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(storePassword))
            {
                _logger.LogError("SSLCommerz credentials not configured");
                return false;
            }

            // Validate with SSLCommerz API
            using (var httpClient = new HttpClient())
            {
                var validationParams = new Dictionary<string, string>
                {
                    { "val_id", request.ValidationId },  // Use val_id from callback, not tran_id
                    { "store_id", storeId },
                    { "store_passwd", storePassword },
                    { "format", "json" }
                };

                var validationContent = new FormUrlEncodedContent(validationParams);
                var validationResponse = await httpClient.PostAsync(validationUrl, validationContent);
                var validationResult = await validationResponse.Content.ReadAsStringAsync();

                _logger.LogInformation($"SSLCommerz validation response: {validationResult}");

                // Check if payment is valid based on status
                if (request.Status?.ToUpper() != "VALID" && request.Status?.ToUpper() != "VALIDATED")
                {
                    _logger.LogWarning($"Payment validation failed with status: {request.Status}");
                    return false;
                }

                // Parse validation response to verify
                if (validationResult.Contains("INVALID_TRANSACTION") || validationResult.Contains("FAILED"))
                {
                    _logger.LogWarning($"SSLCommerz API returned invalid transaction");
                    return false;
                }

                // Create or update payment record
                var payments = await _paymentRepository.GetAllAsync();
                var existingPayment = payments.FirstOrDefault(p => p.PaymentIntentId == request.TransactionId);

                if (existingPayment != null && existingPayment.OrderId.HasValue && existingPayment.OrderId.Value > 0)
                {
                    // Existing payment with order - just update payment status
                    existingPayment.Status = "succeeded";
                    existingPayment.ProcessedAt = DateTime.UtcNow;
                    existingPayment.PaymentMethodDetails = validationResult;

                    // Update order status
                    var order = await _orderRepository.GetByIdAsync(existingPayment.OrderId.Value);
                    if (order != null)
                    {
                        order.PaymentStatus = "Paid";
                        order.PaymentMethod = "sslcommerz";
                        order.PaymentTransactionId = request.TransactionId;
                        order.Status = "Processing";
                        await _orderRepository.UpdateAsync(order);
                        _logger.LogInformation($"Order {existingPayment.OrderId} marked as paid");
                    }

                    await _paymentRepository.UpdateAsync(existingPayment);
                    _logger.LogInformation($"Payment {existingPayment.Id} validated successfully");
                }
                else
                {
                    // Pending payment without order - need to create order from user's cart
                    _logger.LogInformation($"Creating new order for validated transaction: {request.TransactionId}");
                    
                    // existingPayment should be the pending payment
                    if (existingPayment == null)
                    {
                        _logger.LogError($"Cannot create order - no payment found for transaction {request.TransactionId}");
                        return false;
                    }

                    // Get user from pending payment
                    var users = await _userRepository.GetAllAsync();
                    var user = users.FirstOrDefault(u => u.Id == existingPayment.UserId);
                    if (user == null)
                    {
                        _logger.LogError($"Cannot create order - user not found: {existingPayment.UserId}");
                        return false;
                    }
                    
                    _logger.LogInformation($"Found user {user.Id} ({user.Email}) from pending payment");

                    // Get user's cart with items
                    var userCart = await _cartRepository.GetCartWithItemsAsync(user.Id);
                    if (userCart == null)
                    {
                        _logger.LogError($"Cannot create order - cart not found for user {user.Id}");
                        return false;
                    }

                    // Get cart items from the user's cart
                    var cartItems = userCart.CartItems.ToList();
                    
                    if (!cartItems.Any())
                    {
                        _logger.LogError($"Cannot create order - cart is empty for user {user.Id}");
                        return false;
                    }

                    // Calculate order totals
                    var subtotal = cartItems.Sum(c => c.Price * c.Quantity);
                    var shippingCost = 0m; // You can calculate based on rules
                    var tax = subtotal * 0.1m; // 10% tax
                    var total = subtotal + shippingCost + tax;

                    // Find user's default address or create a new one from callback data
                    Core.Entities.Address? shippingAddress = null;
                    
                    // Try to find user's default address
                    var allAddresses = await _addressRepository.GetAllAsync();
                    shippingAddress = allAddresses.FirstOrDefault(a => a.UserId == user.Id && a.IsDefault);
                    
                    // If no default address, try any address for this user
                    if (shippingAddress == null)
                    {
                        shippingAddress = allAddresses.FirstOrDefault(a => a.UserId == user.Id);
                    }
                    
                    // If still no address, create one from SSLCommerz callback data
                    if (shippingAddress == null)
                    {
                        shippingAddress = new Core.Entities.Address
                        {
                            UserId = user.Id,
                            FullName = $"{user.FirstName} {user.LastName}".Trim(),
                            PhoneNumber = user.PhoneNumber ?? "",
                            AddressLine1 = "Address from payment",
                            City = "Dhaka", // Default city
                            State = "Dhaka",
                            PostalCode = "1000",
                            Country = "Bangladesh",
                            IsDefault = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _addressRepository.AddAsync(shippingAddress);
                        _logger.LogInformation($"Created new address {shippingAddress.Id} for user {user.Id}");
                    }
                    
                    var shippingAddressId = shippingAddress.Id;

                    // Create order
                    var order = new Order
                    {
                        OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        UserId = user.Id,
                        ShippingAddressId = shippingAddressId,
                        SubTotal = subtotal,
                        ShippingCost = shippingCost,
                        Tax = tax,
                        Total = total,
                        Status = "Processing",
                        PaymentStatus = "Paid",
                        PaymentMethod = "sslcommerz",
                        PaymentTransactionId = request.TransactionId,
                        CreatedAt = DateTime.UtcNow,
                        OrderItems = cartItems.Select(c => new OrderItem
                        {
                            ProductId = c.ProductId,
                            ProductName = c.Product?.Name ?? "Unknown",
                            ProductSKU = c.Product?.SKU ?? "",
                            Quantity = c.Quantity,
                            Price = c.Price,
                            Subtotal = c.Price * c.Quantity
                        }).ToList()
                    };

                    await _orderRepository.AddAsync(order);
                    _logger.LogInformation($"Order {order.Id} created for user {user.Id}");

                    // Update the pending payment record
                    existingPayment.OrderId = order.Id;
                    existingPayment.Amount = order.Total;
                    existingPayment.Currency = request.currency_type ?? "BDT";
                    existingPayment.Status = "succeeded";
                    existingPayment.ProcessedAt = DateTime.UtcNow;
                    existingPayment.PaymentMethodDetails = validationResult;

                    await _paymentRepository.UpdateAsync(existingPayment);
                    _logger.LogInformation($"Payment record updated for order {order.Id}");

                    // Clear user's cart items
                    foreach (var cartItem in cartItems)
                    {
                        await _cartItemRepository.DeleteAsync(cartItem.Id);
                    }
                    _logger.LogInformation($"Cart cleared for user {user.Id}");
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating SSLCommerz payment: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Handles SSLCommerz payment failure
    /// </summary>
    public async Task HandleSSLCommerzFailureAsync(SSLCommerzValidationRequest request)
    {
        try
        {
            _logger.LogWarning($"Handling SSLCommerz payment failure - Transaction: {request.TransactionId}");

            var payments = await _paymentRepository.GetAllAsync();
            var payment = payments.FirstOrDefault(p => p.PaymentIntentId == request.TransactionId);

            if (payment != null)
            {
                payment.Status = "failed";
                payment.FailedAt = DateTime.UtcNow;
                payment.FailureReason = request.Status ?? "Payment failed";
                await _paymentRepository.UpdateAsync(payment);

                // Update order status
                if (payment.OrderId.HasValue && payment.OrderId.Value > 0)
                {
                    var order = await _orderRepository.GetByIdAsync(payment.OrderId.Value);
                    if (order != null)
                    {
                        order.PaymentStatus = "Failed";
                        await _orderRepository.UpdateAsync(order);
                    }
                }

                _logger.LogInformation($"Payment {payment.Id} marked as failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling SSLCommerz failure: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles SSLCommerz payment cancellation
    /// </summary>
    public async Task HandleSSLCommerzCancellationAsync(SSLCommerzValidationRequest request)
    {
        try
        {
            _logger.LogInformation($"Handling SSLCommerz payment cancellation - Transaction: {request.TransactionId}");

            var payments = await _paymentRepository.GetAllAsync();
            var payment = payments.FirstOrDefault(p => p.PaymentIntentId == request.TransactionId);

            if (payment != null)
            {
                payment.Status = "canceled";
                payment.FailureReason = "Payment cancelled by user";
                await _paymentRepository.UpdateAsync(payment);

                // Update order status
                if (payment.OrderId.HasValue && payment.OrderId.Value > 0)
                {
                    var order = await _orderRepository.GetByIdAsync(payment.OrderId.Value);
                    if (order != null)
                    {
                        order.PaymentStatus = "Pending";
                        order.Status = "Pending";
                        await _orderRepository.UpdateAsync(order);
                    }
                }

                _logger.LogInformation($"Payment {payment.Id} marked as cancelled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling SSLCommerz cancellation: {ex.Message}");
        }
    }

    // Private helper methods

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
            
            // Customer Information
            { "cus_name", request.cus_name },
            { "cus_email", request.cus_email },
            { "cus_phone", request.cus_phone },
            { "cus_add1", request.cus_add1 },
            { "cus_city", request.cus_city },
            { "cus_postcode", request.cus_postcode },
            { "cus_state", request.cus_state },
            { "cus_country", request.cus_country },
            
            // Shipping Information
            { "shipping_method", request.shipping_method },
            { "ship_name", request.ship_name },
            { "ship_add1", request.ship_add1 },
            { "ship_city", request.ship_city },
            { "ship_state", request.ship_state },
            { "ship_postcode", request.ship_postcode },
            { "ship_country", request.ship_country },
            
            // Product Information
            { "product_name", request.product_name },
            { "product_category", request.product_category },
            { "product_profile", request.product_profile }
        };
    }

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
