using Microsoft.EntityFrameworkCore;
using RoadReady.BookingService.Data;
using RoadReady.BookingService.Interfaces;
using RoadReady.BookingService.Models;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.Enums;
using RoadReady.Shared.Responses;
using Microsoft.AspNetCore.Http;

namespace RoadReady.BookingService.Implementations;

public class InspectionService : IInspectionService
{
    private readonly BookingDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<InspectionService> _logger;

    public InspectionService(BookingDbContext context, IFileStorageService fileStorage, ILogger<InspectionService> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<ApiResponse<string>> ProcessCheckOutAsync(int bookingId, Guid agentId, CreateInspectionRequestDto request)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null) return ApiResponse<string>.Fail("Booking not found.");
        
        if (booking.Status != BookingStatus.Confirmed) 
            return ApiResponse<string>.Fail("Only confirmed bookings can be checked out.");

        var now = DateTime.UtcNow;
        var earliestAllowedCheckOut = booking.PickupDate.AddHours(-2); // 2 hour grace period

        if (now < earliestAllowedCheckOut)
        {
            return ApiResponse<string>.Fail($"Too early. Check-out opens 2 hours before pickup ({earliestAllowedCheckOut:g}).");
        }

        if (now > booking.DropoffDate)
        {
            return ApiResponse<string>.Fail("Booking has expired. Cannot check out after the drop-off date.");
        }

        try
        {
            // Save the files to the server
            var vehicleImagePaths = await _fileStorage.SaveFilesAsync(request.VehicleImages ?? new(), "vehicle_images");
            var documentImagePaths = await _fileStorage.SaveFilesAsync(request.DocumentImages ?? new(), "kyc_documents");

            // Create log
            var inspection = new VehicleInspection
            {
                BookingId = bookingId,
                AgentId = agentId,
                Type = InspectionType.CheckOut,
                OdometerReading = request.OdometerReading,
                FuelLevel = request.FuelLevel,
                Notes = request.Notes,
                VehicleImagePaths = string.Join(",", vehicleImagePaths),
                DocumentImagePaths = string.Join(",", documentImagePaths),
                CreatedAt = DateTime.UtcNow
            };

            await _context.VehicleInspections.AddAsync(inspection);

            // 3. Update the Booking Status 
            booking.Status = BookingStatus.Active;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Check-out processed successfully.", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process check-out for booking {BookingId}", bookingId);
            return ApiResponse<string>.Fail("An error occurred during check-out.");
        }
    }

    public async Task<ApiResponse<string>> ProcessCheckInAsync(int bookingId, Guid agentId, CreateInspectionRequestDto request)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null) return ApiResponse<string>.Fail("Booking not found.");
        
        if (booking.Status != BookingStatus.Active) 
            return ApiResponse<string>.Fail("Only active bookings can be checked in.");

      
        if (DateTime.UtcNow < booking.PickupDate.AddHours(-2))
        {
             return ApiResponse<string>.Fail("Invalid return time. This booking has not officially started.");
        }

        try
        {
            var vehicleImagePaths = await _fileStorage.SaveFilesAsync(request.VehicleImages ?? new(), "vehicle_images");

            var inspection = new VehicleInspection
            {
                BookingId = bookingId,
                AgentId = agentId,
                Type = InspectionType.CheckIn,
                OdometerReading = request.OdometerReading,
                FuelLevel = request.FuelLevel,
                Notes = request.Notes,
                VehicleImagePaths = string.Join(",", vehicleImagePaths),
                DocumentImagePaths = "", 
                CreatedAt = DateTime.UtcNow
            };

            await _context.VehicleInspections.AddAsync(inspection);

            booking.Status = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Check-in processed successfully.", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process check-in for booking {BookingId}", bookingId);
            return ApiResponse<string>.Fail("An error occurred during check-in.");
        }
    }
}