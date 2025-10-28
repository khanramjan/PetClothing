using PetClothingShop.Core.DTOs;
using PetClothingShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace PetClothingShop.Infrastructure.Services;

public class TaxService : ITaxService
{
    private readonly ITaxRateRepository _taxRateRepository;
    private readonly ILogger<TaxService> _logger;

    public TaxService(
        ITaxRateRepository taxRateRepository,
        ILogger<TaxService> logger)
    {
        _taxRateRepository = taxRateRepository;
        _logger = logger;
    }

    public async Task<List<TaxRateDTO>> GetTaxRatesAsync()
    {
        var rates = await _taxRateRepository.GetActiveRatesAsync();
        return rates.Select(MapToDTO).ToList();
    }

    public async Task<TaxRateDTO?> GetTaxRateByStateAsync(string stateCode)
    {
        var rate = await _taxRateRepository.GetByStateCodeAsync(stateCode);
        return rate != null ? MapToDTO(rate) : null;
    }

    public async Task<TaxRateDTO> CreateTaxRateAsync(TaxRateDTO request)
    {
        var existing = await _taxRateRepository.GetByStateCodeAsync(request.StateCode);
        if (existing != null)
            throw new InvalidOperationException($"Tax rate for {request.StateCode} already exists");

        var taxRate = new Core.Entities.TaxRate
        {
            StateCode = request.StateCode.ToUpper(),
            StateName = request.StateName,
            TaxPercentage = request.TaxPercentage,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _taxRateRepository.AddAsync(taxRate);
        _logger.LogInformation($"Tax rate created for {taxRate.StateCode}");

        return MapToDTO(taxRate);
    }

    public async Task<TaxRateDTO> UpdateTaxRateAsync(int id, TaxRateDTO request)
    {
        var taxRate = await _taxRateRepository.GetByIdAsync(id);
        if (taxRate == null)
            throw new InvalidOperationException("Tax rate not found");

        taxRate.StateName = request.StateName;
        taxRate.TaxPercentage = request.TaxPercentage;
        taxRate.IsActive = request.IsActive;
        taxRate.UpdatedAt = DateTime.UtcNow;

        await _taxRateRepository.UpdateAsync(taxRate);
        _logger.LogInformation($"Tax rate updated: {taxRate.StateCode}");

        return MapToDTO(taxRate);
    }

    public async Task<bool> DeleteTaxRateAsync(int id)
    {
        var deleted = await _taxRateRepository.DeleteAsync(id);
        if (deleted)
            _logger.LogInformation($"Tax rate deleted: {id}");
        return deleted;
    }

    public async Task<TaxCalculationDTO> CalculateTaxAsync(string stateCode, decimal subtotal)
    {
        var taxRate = await _taxRateRepository.GetByStateCodeAsync(stateCode);
        
        if (taxRate == null)
        {
            _logger.LogWarning($"No tax rate found for state {stateCode}, using default 5%");
            return new TaxCalculationDTO
            {
                Subtotal = subtotal,
                TaxRate = 0.05m,
                TaxAmount = subtotal * 0.05m,
                StateCode = stateCode
            };
        }

        var taxAmount = (subtotal * taxRate.TaxPercentage) / 100;

        return new TaxCalculationDTO
        {
            Subtotal = subtotal,
            TaxRate = taxRate.TaxPercentage / 100,
            TaxAmount = taxAmount,
            StateCode = stateCode
        };
    }

    private TaxRateDTO MapToDTO(Core.Entities.TaxRate taxRate)
    {
        return new TaxRateDTO
        {
            Id = taxRate.Id,
            StateCode = taxRate.StateCode,
            StateName = taxRate.StateName,
            TaxPercentage = taxRate.TaxPercentage,
            IsActive = taxRate.IsActive
        };
    }
}
