using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Interfaces;

public interface ICarService
{
    Task<PagedResponse<CarDto>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<ApiResponse<CarDto>> GetByIdAsync(int id);
    Task<PagedResponse<CarDto>> SearchAsync(CarSearchRequestDto request);
    Task<ApiResponse<CarDto>> CreateAsync(CreateCarRequestDto request);
    Task<ApiResponse<CarDto>> UpdateAsync(int id, UpdateCarRequestDto request);
    Task<ApiResponse<string>> DeleteAsync(int id);
}
