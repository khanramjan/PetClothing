using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PetClothingShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ICheckoutService checkoutService, ILogger<CheckoutController> logger)
    {
        _checkoutService = checkoutService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetCheckoutSummary()
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized("User not authenticated");

            var summary = await _checkoutService.GetCheckoutSummaryAsync(userId.Value);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting checkout summary: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderFromCheckoutRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized("User not authenticated");

            var confirmation = await _checkoutService.CreateOrderAsync(userId.Value, request);
            return Ok(confirmation);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating order: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("calculate-tax")]
    public async Task<IActionResult> CalculateTax([FromBody] TaxCalculationRequest request)
    {
        try
        {
            var calculation = await _checkoutService.CalculateTaxAsync(request.StateCode, request.SubtotalAmount);
            return Ok(calculation);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calculating tax: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("calculate-shipping")]
    public async Task<IActionResult> CalculateShipping([FromBody] ShippingCalculationRequest request)
    {
        try
        {
            var calculation = await _checkoutService.CalculateShippingAsync(
                request.ShippingMethodId,
                request.StateCode,
                request.Weight
            );
            return Ok(calculation);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calculating shipping: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("shipping-methods")]
    public async Task<IActionResult> GetShippingMethods()
    {
        try
        {
            var methods = await _checkoutService.GetAvailableShippingMethodsAsync();
            return Ok(methods);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting shipping methods: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("tax-rates")]
    public async Task<IActionResult> GetTaxRates()
    {
        try
        {
            var rates = await _checkoutService.GetTaxRatesAsync();
            return Ok(rates);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting tax rates: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }
}
