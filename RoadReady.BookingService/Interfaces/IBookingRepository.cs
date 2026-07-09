using RoadReady.BookingService.Models;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.DTOs.Admin;

namespace RoadReady.BookingService.Interfaces;

public interface IBookingRepository
{
    Task<List<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(int id);
    Task<List<Booking>> GetByUserIdAsync(Guid userId);
    Task<List<Payment>> GetPaymentsByUserIdAsync(Guid userId);
    Task<List<int>> GetUnavailableCarIdsAsync(DateTime pickupDate, DateTime dropoffDate);
    Task<bool> HasOverlappingBookingAsync(int carId, DateTime pickupDate, DateTime dropoffDate, int? excludeBookingId = null);
    Task<int> ExpireStalePendingBookingsAsync(TimeSpan olderThan);
    Task<Booking?> GetByIdWithPaymentsAsync(int id);
    Task AddAsync(Booking booking);
    Task<bool> HasCompletedBookingAsync(int carId, Guid userId);
    Task<AdminAnalyticsDto> GetAnalyticsAsync();
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(Booking booking);
    Task SaveAsync();
}