using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewDto>> AddReviewAsync(int carId, Guid userId, CreateReviewRequestDto request);
    Task<ApiResponse<IEnumerable<ReviewDto>>> GetReviewsByCarIdAsync(int carId);
    Task<ApiResponse<ReviewDto>> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewRequestDto request);
    Task<ApiResponse<string>> DeleteReviewAsync(Guid reviewId, Guid userId);
}