using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;
using RoadReady.Shared.DTOs.PromoCode;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Implementations;

public class PromoCodeService : IPromoCodeService
{
    private readonly IPromoCodeRepository _repo;
    private readonly ILogger<PromoCodeService> _logger;

    public PromoCodeService(IPromoCodeRepository repo, ILogger<PromoCodeService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<ApiResponse<List<PromoCodeDto>>> GetAllAsync()
    {
        var promos = await _repo.GetAllAsync();
        return ApiResponse<List<PromoCodeDto>>.Ok(promos.Select(MapToDto).ToList(), "Promo codes fetched successfully.");
    }

    public async Task<ApiResponse<PromoCodeDto>> GetByIdAsync(int id)
    {
        var promo = await _repo.GetByIdAsync(id);
        if (promo == null) return ApiResponse<PromoCodeDto>.Fail("Promo code not found.");
        return ApiResponse<PromoCodeDto>.Ok(MapToDto(promo), "Promo code fetched successfully.");
    }

    public async Task<ApiResponse<PromoCodeDto>> CreateAsync(CreatePromoCodeRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return ApiResponse<PromoCodeDto>.Fail("Code is required.");

        var existing = await _repo.GetByCodeAsync(request.Code);
        if (existing != null)
            return ApiResponse<PromoCodeDto>.Fail("A promo code with this code already exists.");

        var promo = new PromoCode
        {
            Code = request.Code.Trim().ToUpperInvariant(),
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinBookingAmount = request.MinBookingAmount,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            MaxUses = request.MaxUses,
            IsActive = request.IsActive,
            CurrentUses = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(promo);
        await _repo.SaveAsync();

        _logger.LogInformation("Promo code {Code} created.", promo.Code);

        return ApiResponse<PromoCodeDto>.Created(MapToDto(promo), "Promo code created successfully.");
    }

    public async Task<ApiResponse<PromoCodeDto>> UpdateAsync(int id, CreatePromoCodeRequestDto request)
    {
        var promo = await _repo.GetByIdAsync(id);
        if (promo == null) return ApiResponse<PromoCodeDto>.Fail("Promo code not found.");

        promo.Code = request.Code.Trim().ToUpperInvariant();
        promo.Description = request.Description;
        promo.DiscountType = request.DiscountType;
        promo.DiscountValue = request.DiscountValue;
        promo.MinBookingAmount = request.MinBookingAmount;
        promo.ValidFrom = request.ValidFrom;
        promo.ValidUntil = request.ValidUntil;
        promo.MaxUses = request.MaxUses;
        promo.IsActive = request.IsActive;

        await _repo.UpdateAsync(promo);
        await _repo.SaveAsync();

        return ApiResponse<PromoCodeDto>.Ok(MapToDto(promo), "Promo code updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteAsync(int id)
    {
        var promo = await _repo.GetByIdAsync(id);
        if (promo == null) return ApiResponse<string>.Fail("Promo code not found.");

        await _repo.DeleteAsync(promo);
        await _repo.SaveAsync();

        _logger.LogInformation("Promo code {Code} deleted.", promo.Code);

        return ApiResponse<string>.Ok("Promo code deleted successfully.");
    }

    public async Task<ApiResponse<ValidatePromoResponseDto>> ValidateAsync(ValidatePromoRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return ApiResponse<ValidatePromoResponseDto>.Ok(BuildInvalid("Code is required."), "Invalid promo code.");
        }

        var promo = await _repo.GetByCodeAsync(request.Code);
        if (promo == null)
        {
            return ApiResponse<ValidatePromoResponseDto>.Ok(BuildInvalid("Promo code not found."), "Invalid promo code.");
        }

        var now = DateTime.UtcNow;
        if (!promo.IsActive || now < promo.ValidFrom || now > promo.ValidUntil)
        {
            return ApiResponse<ValidatePromoResponseDto>.Ok(BuildInvalid("Promo code is expired or inactive."), "Invalid promo code.");
        }

        if (promo.MaxUses.HasValue && promo.CurrentUses >= promo.MaxUses.Value)
        {
            return ApiResponse<ValidatePromoResponseDto>.Ok(BuildInvalid("Promo code usage limit reached."), "Invalid promo code.");
        }

        if (promo.MinBookingAmount.HasValue && request.BookingAmount < promo.MinBookingAmount.Value)
        {
            return ApiResponse<ValidatePromoResponseDto>.Ok(
                BuildInvalid($"Minimum booking amount of {promo.MinBookingAmount:C} required for this promo code."),
                "Promo code requirements not met.");
        }

        decimal discount = promo.DiscountType switch
        {
            DiscountType.Percentage => Math.Round(request.BookingAmount * promo.DiscountValue / 100m, 2),
            DiscountType.FlatAmount => Math.Min(promo.DiscountValue, request.BookingAmount),
            _ => 0m
        };

        if (discount < 0) discount = 0;

        var response = new ValidatePromoResponseDto
        {
            IsValid = true,
            Code = promo.Code,
            DiscountType = promo.DiscountType,
            DiscountAmount = discount,
            FinalAmount = request.BookingAmount - discount,
            Message = $"You saved {discount:C} on this booking."
        };

        return ApiResponse<ValidatePromoResponseDto>.Ok(response, "Promo code is valid.");
    }

    private static ValidatePromoResponseDto BuildInvalid(string message) => new()
    {
        IsValid = false,
        Message = message
    };

    private static PromoCodeDto MapToDto(PromoCode promo) => new()
    {
        Id = promo.Id,
        Code = promo.Code,
        Description = promo.Description,
        DiscountType = promo.DiscountType,
        DiscountValue = promo.DiscountValue,
        MinBookingAmount = promo.MinBookingAmount,
        ValidFrom = promo.ValidFrom,
        ValidUntil = promo.ValidUntil,
        MaxUses = promo.MaxUses,
        CurrentUses = promo.CurrentUses,
        IsActive = promo.IsActive
    };
}
