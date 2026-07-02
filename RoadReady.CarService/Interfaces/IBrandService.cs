using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Interfaces;

public interface IBrandService
{
    Task<PagedResponse<BrandDto>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<ApiResponse<BrandDto>> GetByIdAsync(int id);
    Task<ApiResponse<BrandDto>> CreateAsync(CreateBrandRequestDto request);
    Task<ApiResponse<BrandDto>> UpdateAsync(int id, CreateBrandRequestDto request);
    Task<ApiResponse<string>> DeleteAsync(int id);
}
