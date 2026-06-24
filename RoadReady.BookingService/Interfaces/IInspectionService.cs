using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.Responses;

namespace RoadReady.BookingService.Interfaces;

public interface IInspectionService
{
    Task<ApiResponse<string>> ProcessCheckOutAsync(int bookingId, Guid agentId, CreateInspectionRequestDto request);
    Task<ApiResponse<string>> ProcessCheckInAsync(int bookingId, Guid agentId, CreateInspectionRequestDto request);
}