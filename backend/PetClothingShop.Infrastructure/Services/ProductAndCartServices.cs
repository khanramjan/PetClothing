using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using PetClothingShop.Infrastructure.Data;

namespace PetClothingShop.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ApplicationDbContext _context;

    public ProductService(IProductRepository productRepository, ApplicationDbContext context)
    {
        _productRepository = productRepository;
        _context = context;
    }

    public async Task<PagedResult<ProductDTO>> GetProductsAsync(ProductFilterRequest filter)
    {
        var (products, totalCount) = await _productRepository.GetFilteredProductsAsync(
            filter.SearchTerm,
            filter.CategoryId,
            filter.PetType,
            filter.Size,
            filter.MinPrice,
            filter.MaxPrice,
            filter.IsFeatured,
            filter.SortBy,
            filter.Page,
            filter.PageSize
        );

        var productDTOs = products.Select(p => MapToDTO(p)).ToList();

        return new PagedResult<ProductDTO>
        {
            Items = productDTOs,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ProductDTO?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdWithImagesAsync(id);
        return product != null ? MapToDTO(product) : null;
    }

    public async Task<ProductDTO> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            SKU = request.SKU,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            PetType = request.PetType,
            Size = request.Size,
            Color = request.Color,
            Material = request.Material,
            IsFeatured = request.IsFeatured,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product);

        // Add product images if provided
        if (request.Images != null && request.Images.Any())
        {
            var images = request.Images.Select(img => new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = img.ImageUrl,
                AltText = img.AltText,
                IsPrimary = img.IsPrimary,
                DisplayOrder = img.DisplayOrder
            }).ToList();

            await _context.ProductImages.AddRangeAsync(images);
            await _context.SaveChangesAsync();
        }

        return MapToDTO(product);
    }

    public async Task<ProductDTO> UpdateProductAsync(UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.SKU = request.SKU;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.PetType = request.PetType;
        product.Size = request.Size;
        product.Color = request.Color;
        product.Material = request.Material;
        product.IsFeatured = request.IsFeatured;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);

        // Update product images if provided
        if (request.Images != null && request.Images.Any())
        {
            // Remove old images
            var oldImages = _context.ProductImages.Where(pi => pi.ProductId == product.Id);
            _context.ProductImages.RemoveRange(oldImages);

            // Add new images
            var images = request.Images.Select(img => new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = img.ImageUrl,
                AltText = img.AltText,
                IsPrimary = img.IsPrimary,
                DisplayOrder = img.DisplayOrder
            }).ToList();

            await _context.ProductImages.AddRangeAsync(images);
            await _context.SaveChangesAsync();
        }

        return MapToDTO(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    public async Task<List<ProductDTO>> GetFeaturedProductsAsync(int count = 8)
    {
        var products = await _productRepository.GetFeaturedProductsAsync(count);
        return products.Select(p => MapToDTO(p)).ToList();
    }

    private ProductDTO MapToDTO(Product product)
    {
        return new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            SKU = product.SKU,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            PetType = product.PetType,
            Size = product.Size,
            Color = product.Color,
            Material = product.Material,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            Rating = product.Rating,
            ReviewCount = product.ReviewCount,
            Images = product.Images?.Select(i => new ProductImageDTO
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList() ?? new List<ProductImageDTO>(),
            CreatedAt = product.CreatedAt
        };
    }
}

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<CartDTO> GetCartAsync(int userId)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart == null)
        {
            return new CartDTO { Id = 0, Items = new List<CartItemDTO>(), SubTotal = 0, TotalItems = 0 };
        }

        return MapToDTO(cart);
    }

    public async Task<CartDTO> AddToCartAsync(int userId, AddToCartRequest request)
    {
        var product = await _productRepository.GetByIdWithImagesAsync(request.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        if (product.StockQuantity < request.Quantity)
        {
            throw new InvalidOperationException("Insufficient stock");
        }

        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            await _cartRepository.AddAsync(cart);
        }

        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            cart.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Price = product.DiscountPrice ?? product.Price,
                AddedAt = DateTime.UtcNow
            });
            cart.UpdatedAt = DateTime.UtcNow;
        }

        await _cartRepository.UpdateAsync(cart);
        return MapToDTO(cart);
    }

    public async Task<CartDTO> UpdateCartItemAsync(int userId, UpdateCartItemRequest request)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart == null)
        {
            throw new InvalidOperationException("Cart not found");
        }

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == request.CartItemId);
        if (cartItem == null)
        {
            throw new InvalidOperationException("Cart item not found");
        }

        if (request.Quantity <= 0)
        {
            cart.CartItems.Remove(cartItem);
        }
        else
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null || product.StockQuantity < request.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock");
            }

            cartItem.Quantity = request.Quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart);
        return MapToDTO(cart);
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart == null) return false;

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
        if (cartItem == null) return false;

        cart.CartItems.Remove(cartItem);
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart);
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart == null) return false;

        cart.CartItems.Clear();
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart);
        return true;
    }

    private CartDTO MapToDTO(Cart cart)
    {
        var items = cart.CartItems.Select(ci => new CartItemDTO
        {
            Id = ci.Id,
            ProductId = ci.ProductId,
            ProductName = ci.Product.Name,
            ProductImage = ci.Product.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? 
                          ci.Product.Images?.FirstOrDefault()?.ImageUrl ?? string.Empty,
            Price = ci.Price,
            Quantity = ci.Quantity,
            Subtotal = ci.Price * ci.Quantity,
            StockQuantity = ci.Product.StockQuantity
        }).ToList();

        return new CartDTO
        {
            Id = cart.Id,
            Items = items,
            SubTotal = items.Sum(i => i.Subtotal),
            TotalItems = items.Sum(i => i.Quantity)
        };
    }
}
