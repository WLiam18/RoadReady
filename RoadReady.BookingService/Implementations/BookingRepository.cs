using Microsoft.EntityFrameworkCore;
using RoadReady.BookingService.Data;
using RoadReady.BookingService.Interfaces;
using RoadReady.BookingService.Models;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.DTOs.Admin;

using RoadReady.Shared.Enums;

namespace RoadReady.BookingService.Implementations;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Booking>> GetAllAsync()
    {
        return await _context.Bookings
            .Include(b => b.Payments) 
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasCompletedBookingAsync(int carId, Guid userId)
    {
    
    return await _context.Bookings.AnyAsync(b => 
        b.CarId == carId && 
        b.UserId == userId && 
        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed));
    }
    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Payments) 
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Booking>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Bookings
            .Include(b => b.Payments) 
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingBookingAsync(int carId, DateTime pickupDate, DateTime dropoffDate, int? excludeBookingId = null)
    {
        return await _context.Bookings.AnyAsync(b =>
            b.CarId == carId &&
            b.Status != BookingStatus.Cancelled && 
            (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value) && 
            pickupDate < b.DropoffDate &&
            dropoffDate > b.PickupDate);
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }

    public Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Booking booking)
    {
        _context.Bookings.Remove(booking);
        return Task.CompletedTask;
    }

    public async Task SaveAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("A database error occurred while saving the booking.", ex);
        }
    }

    public async Task<AdminAnalyticsDto> GetAnalyticsAsync()
    {
        var totalReservations = await _context.Bookings.CountAsync();
        
        var activeBookings = await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active);
            
        var cancelledBookings = await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Cancelled);

        var totalRevenue = await _context.Payments
            .Where(p => p.Type == PaymentType.InitialCharge && p.Status == PaymentStatus.Succeeded)
            .SumAsync(p => p.Amount);

        var totalRefunded = await _context.Payments
            .Where(p => p.Type == PaymentType.Refund && p.Status == PaymentStatus.Succeeded)
            .SumAsync(p => p.Amount);

        return new AdminAnalyticsDto
        {
            TotalReservations = totalReservations,
            ActiveBookings = activeBookings,
            CancelledBookings = cancelledBookings,
            TotalRevenue = totalRevenue,
            TotalRefunded = totalRefunded,
            NetRevenue = totalRevenue - totalRefunded
        };
    }
}