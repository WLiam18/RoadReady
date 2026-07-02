using RoadReady.BookingService.Models;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.DTOs.Admin;
using RoadReady.Shared.DTOs.Payment;
using RoadReady.Shared.Responses;

namespace RoadReady.BookingService.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<BookingDto>> CreateAsync(Guid userId, CreateBookingRequestDto request);
    Task<ApiResponse<List<BookingDto>>> GetAllAsync();
    Task<ApiResponse<BookingDto>> GetByIdAsync(int id, Guid currentUserId, bool isAdmin);
    Task<ApiResponse<List<BookingDto>>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<List<PaymentDto>>> GetPaymentsByUserIdAsync(Guid userId);
    Task<ApiResponse<AdminAnalyticsDto>> GetAdminAnalyticsAsync(bool isAdmin);
    Task<ApiResponse<bool>> VerifyReviewEligibilityAsync(int carId, Guid userId);
    Task<List<int>> GetUnavailableCarIdsAsync(DateTime pickupDate, DateTime dropoffDate);
    Task<ApiResponse<BookingDto>> CancelAsync(int id, Guid currentUserId, bool isAdmin);
    Task<ApiResponse<BookingDto>> ModifyAsync(int id, ModifyBookingRequestDto request, Guid currentUserId, bool isAdmin);
    Task<Booking?> GetByIdForReceiptAsync(int id);
}
