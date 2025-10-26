using Microsoft.EntityFrameworkCore;
using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;

namespace PetClothingShop.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAddressRepository _addressRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IAddressRepository addressRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _addressRepository = addressRepository;
    }

    public async Task<OrderDTO> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart == null || !cart.CartItems.Any())
        {
            throw new InvalidOperationException("Cart is empty");
        }

        var address = await _addressRepository.GetByIdAsync(request.ShippingAddressId);
        if (address == null || address.UserId != userId)
        {
            throw new InvalidOperationException("Invalid shipping address");
        }

        // Calculate totals
        var subTotal = cart.CartItems.Sum(ci => ci.Price * ci.Quantity);
        var shippingCost = subTotal > 50 ? 0 : 9.99m;
        var tax = subTotal * 0.1m; // 10% tax
        var total = subTotal + shippingCost + tax;

        // Create order
        var orderNumber = await _orderRepository.GenerateOrderNumberAsync();
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            ShippingAddressId = request.ShippingAddressId,
            SubTotal = subTotal,
            ShippingCost = shippingCost,
            Tax = tax,
            Total = total,
            Status = "Pending",
            PaymentStatus = "Pending",
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Create order items
        foreach (var cartItem in cart.CartItems)
        {
            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                ProductSKU = cartItem.Product.SKU,
                Quantity = cartItem.Quantity,
                Price = cartItem.Price,
                Subtotal = cartItem.Price * cartItem.Quantity
            };
            order.OrderItems.Add(orderItem);

            // Update product stock
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product != null)
            {
                product.StockQuantity -= cartItem.Quantity;
                await _productRepository.UpdateAsync(product);
            }
        }

        await _orderRepository.AddAsync(order);

        // Clear cart
        cart.CartItems.Clear();
        await _cartRepository.UpdateAsync(cart);

        return await MapToDTO(order);
    }

    public async Task<OrderDTO?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
        if (order == null || order.UserId != userId)
        {
            return null;
        }
        return await MapToDTO(order);
    }

    public async Task<List<OrderDTO>> GetUserOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetUserOrdersAsync(userId);
        var orderDTOs = new List<OrderDTO>();
        foreach (var order in orders)
        {
            orderDTOs.Add(await MapToDTO(order));
        }
        return orderDTOs;
    }

    public async Task<PagedResult<OrderDTO>> GetAllOrdersAsync(int page = 1, int pageSize = 20)
    {
        var (orders, totalCount) = await _orderRepository.GetAllOrdersPagedAsync(page, pageSize);
        var orderDTOs = new List<OrderDTO>();
        foreach (var order in orders)
        {
            orderDTOs.Add(await MapToDTO(order));
        }

        return new PagedResult<OrderDTO>
        {
            Items = orderDTOs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        order.Status = request.Status;
        
        if (request.Status == "Shipped" && !order.ShippedAt.HasValue)
        {
            order.ShippedAt = DateTime.UtcNow;
        }
        else if (request.Status == "Delivered")
        {
            order.DeliveredAt = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);
        return true;
    }

    public async Task<bool> CancelOrderAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
        if (order == null || order.UserId != userId)
        {
            return false;
        }

        if (order.Status != "Pending")
        {
            throw new InvalidOperationException("Cannot cancel order that is already processing");
        }

        order.Status = "Cancelled";
        
        // Restore product stock
        foreach (var item in order.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                await _productRepository.UpdateAsync(product);
            }
        }

        await _orderRepository.UpdateAsync(order);
        return true;
    }

    private async Task<OrderDTO> MapToDTO(Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            SubTotal = order.SubTotal,
            ShippingCost = order.ShippingCost,
            Tax = order.Tax,
            Total = order.Total,
            ShippingAddress = new AddressDTO
            {
                Id = order.ShippingAddress.Id,
                FullName = order.ShippingAddress.FullName,
                PhoneNumber = order.ShippingAddress.PhoneNumber,
                AddressLine1 = order.ShippingAddress.AddressLine1,
                AddressLine2 = order.ShippingAddress.AddressLine2,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country,
                IsDefault = order.ShippingAddress.IsDefault
            },
            Items = order.OrderItems.Select(oi => new OrderItemDTO
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                ProductSKU = oi.ProductSKU,
                ProductImage = oi.Product?.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? 
                              oi.Product?.Images?.FirstOrDefault()?.ImageUrl ?? string.Empty,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Subtotal = oi.Subtotal
            }).ToList(),
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt
        };
    }
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDTO>> GetCategoriesAsync()
    {
        var categories = await _categoryRepository.GetCategoriesWithSubCategoriesAsync();
        return categories.Select(MapToDTO).ToList();
    }

    public async Task<CategoryDTO?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? MapToDTO(category) : null;
    }

    public async Task<CategoryDTO> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepository.AddAsync(category);
        return MapToDTO(category);
    }

    public async Task<CategoryDTO> UpdateCategoryAsync(int id, CreateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new InvalidOperationException("Category not found");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;
        category.DisplayOrder = request.DisplayOrder;

        await _categoryRepository.UpdateAsync(category);
        return MapToDTO(category);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }

    private CategoryDTO MapToDTO(Category category)
    {
        return new CategoryDTO
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive,
            DisplayOrder = category.DisplayOrder,
            SubCategories = category.SubCategories?.Select(MapToDTO).ToList() ?? new List<CategoryDTO>()
        };
    }
}

public class UserService : IUserService
{
    private readonly IAddressRepository _addressRepository;

    public UserService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<List<AddressDTO>> GetUserAddressesAsync(int userId)
    {
        var addresses = await _addressRepository.GetUserAddressesAsync(userId);
        return addresses.Select(MapToDTO).ToList();
    }

    public async Task<AddressDTO> AddAddressAsync(int userId, CreateAddressRequest request)
    {
        var address = new Address
        {
            UserId = userId,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        if (request.IsDefault)
        {
            var existingAddresses = await _addressRepository.GetUserAddressesAsync(userId);
            foreach (var existingAddress in existingAddresses)
            {
                existingAddress.IsDefault = false;
                await _addressRepository.UpdateAsync(existingAddress);
            }
        }

        await _addressRepository.AddAsync(address);
        return MapToDTO(address);
    }

    public async Task<AddressDTO> UpdateAddressAsync(int userId, int addressId, CreateAddressRequest request)
    {
        var address = await _addressRepository.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId)
        {
            throw new InvalidOperationException("Address not found");
        }

        address.FullName = request.FullName;
        address.PhoneNumber = request.PhoneNumber;
        address.AddressLine1 = request.AddressLine1;
        address.AddressLine2 = request.AddressLine2;
        address.City = request.City;
        address.State = request.State;
        address.PostalCode = request.PostalCode;
        address.Country = request.Country;
        address.IsDefault = request.IsDefault;

        if (request.IsDefault)
        {
            var existingAddresses = await _addressRepository.GetUserAddressesAsync(userId);
            foreach (var existingAddress in existingAddresses.Where(a => a.Id != addressId))
            {
                existingAddress.IsDefault = false;
                await _addressRepository.UpdateAsync(existingAddress);
            }
        }

        await _addressRepository.UpdateAsync(address);
        return MapToDTO(address);
    }

    public async Task<bool> DeleteAddressAsync(int userId, int addressId)
    {
        var address = await _addressRepository.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId)
        {
            return false;
        }

        return await _addressRepository.DeleteAsync(addressId);
    }

    public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
    {
        var address = await _addressRepository.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId)
        {
            return false;
        }

        var existingAddresses = await _addressRepository.GetUserAddressesAsync(userId);
        foreach (var existingAddress in existingAddresses)
        {
            existingAddress.IsDefault = existingAddress.Id == addressId;
            await _addressRepository.UpdateAsync(existingAddress);
        }

        return true;
    }

    private AddressDTO MapToDTO(Address address)
    {
        return new AddressDTO
        {
            Id = address.Id,
            FullName = address.FullName,
            PhoneNumber = address.PhoneNumber,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country,
            IsDefault = address.IsDefault
        };
    }
}

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;

    public ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }

    public async Task<List<ReviewDTO>> GetProductReviewsAsync(int productId)
    {
        var reviews = await _reviewRepository.GetProductReviewsAsync(productId);
        return reviews.Select(r => new ReviewDTO
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserName = $"{r.User.FirstName} {r.User.LastName}",
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Comment,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<ReviewDTO> CreateReviewAsync(int userId, CreateReviewRequest request)
    {
        var hasPurchased = await _reviewRepository.HasUserPurchasedProductAsync(userId, request.ProductId);

        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = userId,
            Rating = request.Rating,
            Title = request.Title,
            Comment = request.Comment,
            IsVerifiedPurchase = hasPurchased,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review);

        // Update product rating
        await UpdateProductRating(request.ProductId);

        return new ReviewDTO
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserName = string.Empty,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<bool> DeleteReviewAsync(int userId, int reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null || review.UserId != userId)
        {
            return false;
        }

        var productId = review.ProductId;
        var result = await _reviewRepository.DeleteAsync(reviewId);
        
        if (result)
        {
            await UpdateProductRating(productId);
        }

        return result;
    }

    public async Task<bool> ApproveReviewAsync(int reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return false;

        review.IsApproved = true;
        await _reviewRepository.UpdateAsync(review);
        return true;
    }

    private async Task UpdateProductRating(int productId)
    {
        var reviews = await _reviewRepository.GetProductReviewsAsync(productId);
        var product = await _productRepository.GetByIdAsync(productId);
        
        if (product != null)
        {
            product.ReviewCount = reviews.Count;
            product.Rating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;
            await _productRepository.UpdateAsync(product);
        }
    }
}

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public DashboardService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
    {
        var allOrders = await _orderRepository.GetAllAsync();
        var allProducts = await _productRepository.GetAllAsync();
        var allUsers = await _userRepository.GetAllAsync();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        return new DashboardStatsDTO
        {
            TotalOrders = allOrders.Count,
            PendingOrders = allOrders.Count(o => o.Status == "Pending"),
            TotalProducts = allProducts.Count,
            LowStockProducts = allProducts.Count(p => p.StockQuantity < 10),
            TotalCustomers = allUsers.Count(u => u.Role == "Customer"),
            NewCustomersThisMonth = allUsers.Count(u => u.Role == "Customer" && u.CreatedAt >= monthStart),
            TotalRevenue = allOrders.Where(o => o.PaymentStatus == "Paid").Sum(o => o.Total),
            MonthlyRevenue = allOrders.Where(o => o.PaymentStatus == "Paid" && o.CreatedAt >= monthStart).Sum(o => o.Total),
            OrdersByStatus = allOrders.GroupBy(o => o.Status)
                .Select(g => new OrderStatusCount { Status = g.Key, Count = g.Count() })
                .ToList(),
            RevenueChart = GetRevenueChart(allOrders),
            TopProducts = GetTopProducts(allOrders)
        };
    }

    private List<RevenueByMonth> GetRevenueChart(List<Order> orders)
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        return orders
            .Where(o => o.PaymentStatus == "Paid" && o.CreatedAt >= sixMonthsAgo)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new RevenueByMonth
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Revenue = g.Sum(o => o.Total)
            })
            .OrderBy(r => r.Month)
            .ToList();
    }

    private List<TopProductDTO> GetTopProducts(List<Order> orders)
    {
        return orders
            .Where(o => o.PaymentStatus == "Paid")
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new { oi.ProductId, oi.ProductName })
            .Select(g => new TopProductDTO
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                ImageUrl = g.First().Product?.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? string.Empty,
                TotalSold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Subtotal)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();
    }
}
