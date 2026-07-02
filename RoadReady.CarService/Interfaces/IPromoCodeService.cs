using RoadReady.CarService.Models;
using RoadReady.Shared.DTOs.PromoCode;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Interfaces;

public interface IPromoCodeService
{
    Task<ApiResponse<List<PromoCodeDto>>> GetAllAsync();
    Task<ApiResponse<PromoCodeDto>> GetByIdAsync(int id);
    Task<ApiResponse<PromoCodeDto>> CreateAsync(CreatePromoCodeRequestDto request);
    Task<ApiResponse<PromoCodeDto>> UpdateAsync(int id, CreatePromoCodeRequestDto request);
    Task<ApiResponse<string>> DeleteAsync(int id);
    Task<ApiResponse<ValidatePromoResponseDto>> ValidateAsync(ValidatePromoRequestDto request);
}
