using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewDto>> AddReviewAsync(int carId, Guid userId, CreateReviewRequestDto request);
    Task<ApiResponse<IEnumerable<ReviewDto>>> GetReviewsByCarIdAsync(int carId);
}