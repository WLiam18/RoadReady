using System.Net.Http;
using System.Net.Http.Json;
using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Implementations;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICarRepository _carRepository;
    private readonly ILogger<ReviewService> _logger;
    private readonly HttpClient _httpClient;
    public ReviewService(
        IReviewRepository reviewRepository, 
        ICarRepository carRepository, 
        ILogger<ReviewService> logger,
        HttpClient httpClient) 
    {
        _reviewRepository = reviewRepository;
        _carRepository = carRepository;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<ReviewDto>> AddReviewAsync(int carId, Guid userId, CreateReviewRequestDto request)
    {
        var car = await _carRepository.GetByIdAsync(carId); 
        if (car == null)
        {
            return ApiResponse<ReviewDto>.Fail("Car not found.");
        }
        try
        {
           
            var verifyUrl = $"http://localhost:5003/api/v1/bookings/verify-eligibility?userId={userId}&carId={carId}";
            var bookingResponse = await _httpClient.GetAsync(verifyUrl);

            if (!bookingResponse.IsSuccessStatusCode)
            {
                return ApiResponse<ReviewDto>.Fail("You can only review a car after you have successfully booked and driven it.");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with BookingService for verification.");
            return ApiResponse<ReviewDto>.Fail("Unable to verify booking history at this time. Please try again later.");
        }

        var hasReviewed = await _reviewRepository.HasUserReviewedCarAsync(carId, userId);
        if (hasReviewed)
        {
            return ApiResponse<ReviewDto>.Fail("You have already reviewed this car.");
        }

        var review = new Review
        {
            CarId = carId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddReviewAsync(review);
        await _reviewRepository.SaveChangesAsync();

        _logger.LogInformation("Review added for CarId: {CarId} by UserId: {UserId}", carId, userId);

        var reviewDto = new ReviewDto
        {
            Id = review.Id,
            CarId = review.CarId,
            UserId = review.UserId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return ApiResponse<ReviewDto>.Created(reviewDto, "Review added successfully.");
    }

    public async Task<ApiResponse<IEnumerable<ReviewDto>>> GetReviewsByCarIdAsync(int carId)
    {
        var reviews = await _reviewRepository.GetReviewsByCarIdAsync(carId);

        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            CarId = r.CarId,
            UserId = r.UserId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });

        return ApiResponse<IEnumerable<ReviewDto>>.Ok(reviewDtos, "Reviews fetched successfully.");
    }
}