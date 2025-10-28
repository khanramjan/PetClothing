using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Interfaces;

namespace PetClothingShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // For testing without auth, use user ID 1 as default
        return int.TryParse(userIdClaim, out var userId) ? userId : 1;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(new { success = true, data = cart });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart");
            return StatusCode(500, new { success = false, message = "An error occurred while fetching the cart" });
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            var userId = GetUserId();
            var cart = await _cartService.AddToCartAsync(userId, request);
            return Ok(new { success = true, data = cart });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to cart");
            return StatusCode(500, new { success = false, message = "An error occurred while adding to cart" });
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        try
        {
            var userId = GetUserId();
            var cart = await _cartService.UpdateCartItemAsync(userId, request);
            return Ok(new { success = true, data = cart });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item");
            return StatusCode(500, new { success = false, message = "An error occurred while updating cart item" });
        }
    }

    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            if (!result)
            {
                return NotFound(new { success = false, message = "Cart item not found" });
            }
            return Ok(new { success = true, message = "Item removed from cart" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cart");
            return StatusCode(500, new { success = false, message = "An error occurred while removing from cart" });
        }
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            var userId = GetUserId();
            var result = await _cartService.ClearCartAsync(userId);
            return Ok(new { success = true, message = "Cart cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
            return StatusCode(500, new { success = false, message = "An error occurred while clearing cart" });
        }
    }
}
