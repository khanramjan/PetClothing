using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Entities;
using PetClothingShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace PetClothingShop.Infrastructure.Services;

public class ShippingService : IShippingService
{
    private readonly IShippingMethodRepository _shippingMethodRepository;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(
        IShippingMethodRepository shippingMethodRepository,
        ILogger<ShippingService> logger)
    {
        _shippingMethodRepository = shippingMethodRepository;
        _logger = logger;
    }

    public async Task<List<ShippingMethodDTO>> GetShippingMethodsAsync()
    {
        var methods = await _shippingMethodRepository.GetActiveShippingMethodsAsync();
        return methods.Select(MapToDTO).ToList();
    }

    public async Task<ShippingMethodDTO?> GetShippingMethodByIdAsync(int id)
    {
        var method = await _shippingMethodRepository.GetByIdAsync(id);
        return method != null ? MapToDTO(method) : null;
    }

    public async Task<ShippingMethodDTO> CreateShippingMethodAsync(ShippingMethodDTO request)
    {
        var existing = await _shippingMethodRepository.GetByNameAsync(request.Name);
        if (existing != null)
            throw new InvalidOperationException($"Shipping method '{request.Name}' already exists");

        var method = new ShippingMethod
        {
            Name = request.Name,
            Description = request.Description,
            BaseCost = request.BaseCost,
            MinDeliveryDays = request.MinDeliveryDays,
            MaxDeliveryDays = request.MaxDeliveryDays,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _shippingMethodRepository.AddAsync(method);
        _logger.LogInformation($"Shipping method created: {method.Name}");

        return MapToDTO(method);
    }

    public async Task<ShippingMethodDTO> UpdateShippingMethodAsync(int id, ShippingMethodDTO request)
    {
        var method = await _shippingMethodRepository.GetByIdAsync(id);
        if (method == null)
            throw new InvalidOperationException("Shipping method not found");

        method.Description = request.Description;
        method.BaseCost = request.BaseCost;
        method.MinDeliveryDays = request.MinDeliveryDays;
        method.MaxDeliveryDays = request.MaxDeliveryDays;
        method.IsActive = request.IsActive;
        method.UpdatedAt = DateTime.UtcNow;

        await _shippingMethodRepository.UpdateAsync(method);
        _logger.LogInformation($"Shipping method updated: {method.Name}");

        return MapToDTO(method);
    }

    public async Task<bool> DeleteShippingMethodAsync(int id)
    {
        var deleted = await _shippingMethodRepository.DeleteAsync(id);
        if (deleted)
            _logger.LogInformation($"Shipping method deleted: {id}");
        return deleted;
    }

    public async Task<ShippingCalculationDTO> CalculateShippingCostAsync(int shippingMethodId, decimal weight, string stateCode)
    {
        var method = await _shippingMethodRepository.GetByIdAsync(shippingMethodId);
        if (method == null)
            throw new InvalidOperationException("Shipping method not found");

        var shippingCost = method.BaseCost;

        if (method.CostPerWeight.HasValue && method.CostPerWeight > 0)
        {
            shippingCost += (weight * method.CostPerWeight.Value);
        }

        int deliveryDays = method.MinDeliveryDays + 2;
        var estimatedDelivery = DateTime.UtcNow.AddDays(deliveryDays);

        return new ShippingCalculationDTO
        {
            ShippingMethodId = method.Id,
            ShippingMethodName = method.Name,
            ShippingCost = shippingCost,
            MinDeliveryDays = method.MinDeliveryDays,
            MaxDeliveryDays = method.MaxDeliveryDays,
            EstimatedDeliveryDate = estimatedDelivery
        };
    }

    private ShippingMethodDTO MapToDTO(ShippingMethod method)
    {
        return new ShippingMethodDTO
        {
            Id = method.Id,
            Name = method.Name,
            Description = method.Description,
            BaseCost = method.BaseCost,
            MinDeliveryDays = method.MinDeliveryDays,
            MaxDeliveryDays = method.MaxDeliveryDays,
            IsActive = method.IsActive
        };
    }
}
