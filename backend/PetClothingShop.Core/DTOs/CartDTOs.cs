namespace PetClothingShop.Core.DTOs;

using System.Text.Json.Serialization;

public class CartDTO
{
    public int Id { get; set; }
    public List<CartItemDTO> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public int TotalItems { get; set; }
}

public class CartItemDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public int StockQuantity { get; set; }
}

public class AddToCartRequest
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }
    
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [JsonPropertyName("cartItemId")]
    public int CartItemId { get; set; }
    
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}
