using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;
using RoadReady.Shared.DTOs.Auth;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Implementations;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICarRepository _carRepository;
    private readonly ILogger<ReviewService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _authServiceBaseUrl;
    private readonly string _bookingServiceBaseUrl;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ReviewService(
        IReviewRepository reviewRepository,
        ICarRepository carRepository,
        ILogger<ReviewService> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _reviewRepository = reviewRepository;
        _carRepository = carRepository;
        _logger = logger;
        _httpClient = httpClient;
        _authServiceBaseUrl = configuration["Services:AuthServiceBaseUrl"] ?? "http://localhost:5001";
        _bookingServiceBaseUrl = configuration["Services:BookingServiceBaseUrl"] ?? "http://localhost:5003";
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
            var verifyUrl = $"{_bookingServiceBaseUrl.TrimEnd('/')}/api/v1/bookings/verify-eligibility?userId={userId}&carId={carId}";
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

        var userInfo = await FetchUserInfoAsync(userId);

        var reviewDto = new ReviewDto
        {
            Id = review.Id,
            CarId = review.CarId,
            UserId = review.UserId,
            UserName = userInfo?.UserName ?? "Unknown",
            UserProfileImage = userInfo?.ProfileImageUrl,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return ApiResponse<ReviewDto>.Created(reviewDto, "Review added successfully.");
    }

    public async Task<ApiResponse<IEnumerable<ReviewDto>>> GetReviewsByCarIdAsync(int carId)
    {
        var reviews = await _reviewRepository.GetReviewsByCarIdAsync(carId);

        var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
        var userInfos = await FetchUserInfosAsync(userIds);

        var reviewDtos = reviews.Select(r =>
        {
            userInfos.TryGetValue(r.UserId, out var userInfo);
            return new ReviewDto
            {
                Id = r.Id,
                CarId = r.CarId,
                UserId = r.UserId,
                UserName = userInfo?.UserName ?? "Unknown",
                UserProfileImage = userInfo?.ProfileImageUrl,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            };
        });

        return ApiResponse<IEnumerable<ReviewDto>>.Ok(reviewDtos, "Reviews fetched successfully.");
    }

    public async Task<ApiResponse<ReviewDto>> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewRequestDto request)
    {
        var review = await _reviewRepository.GetReviewByIdAsync(reviewId);

        if (review == null)
        {
            return ApiResponse<ReviewDto>.Fail("Review not found.");
        }

        if (review.UserId != userId)
        {
            return ApiResponse<ReviewDto>.Fail("You can only edit your own reviews.");
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment;

        await _reviewRepository.UpdateReviewAsync(review);
        await _reviewRepository.SaveChangesAsync();

        _logger.LogInformation("Review {ReviewId} updated by UserId: {UserId}", reviewId, userId);

        var userInfo = await FetchUserInfoAsync(userId);

        var reviewDto = new ReviewDto
        {
            Id = review.Id,
            CarId = review.CarId,
            UserId = review.UserId,
            UserName = userInfo?.UserName ?? "Unknown",
            UserProfileImage = userInfo?.ProfileImageUrl,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return ApiResponse<ReviewDto>.Ok(reviewDto, "Review updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        var review = await _reviewRepository.GetReviewByIdAsync(reviewId);

        if (review == null)
        {
            return ApiResponse<string>.Fail("Review not found.");
        }

        if (review.UserId != userId)
        {
            return ApiResponse<string>.Fail("You can only delete your own reviews.");
        }

        await _reviewRepository.DeleteReviewAsync(review);
        await _reviewRepository.SaveChangesAsync();

        _logger.LogInformation("Review {ReviewId} deleted by UserId: {UserId}", reviewId, userId);

        return ApiResponse<string>.Ok("Review deleted successfully.");
    }

    private async Task<UserInfo?> FetchUserInfoAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_authServiceBaseUrl}/api/v1/auth/users/{userId}");
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(_jsonOptions);
            if (result?.Data == null) return null;

            return new UserInfo
            {
                UserName = $"{result.Data.FirstName} {result.Data.LastName}".Trim(),
                ProfileImageUrl = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch user info for UserId: {UserId}", userId);
            return null;
        }
    }

    private async Task<Dictionary<Guid, UserInfo>> FetchUserInfosAsync(List<Guid> userIds)
    {
        var dict = new Dictionary<Guid, UserInfo>();
        foreach (var uid in userIds)
        {
            var info = await FetchUserInfoAsync(uid);
            if (info != null)
                dict[uid] = info;
        }
        return dict;
    }

    private class UserInfo
    {
        public string UserName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }
}