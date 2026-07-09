using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadReady.BookingService.Interfaces;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.Responses;

namespace RoadReady.BookingService.Controllers;

[ApiController]
[Route("api/v1/bookings/{bookingId}/inspections")]
[Authorize(Roles = "RentalAgent,Admin")]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionService _inspectionService;
    private readonly IValidator<CreateInspectionRequestDto> _validator;

    public InspectionsController(IInspectionService inspectionService, IValidator<CreateInspectionRequestDto> validator)
    {
        _inspectionService = inspectionService;
        _validator = validator;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CheckOut(int bookingId, [FromForm] CreateInspectionRequestDto request)
    {
        var agentIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(agentIdString, out var agentId)) return Unauthorized();

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var result = await _inspectionService.ProcessCheckOutAsync(bookingId, agentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn(int bookingId, [FromForm] CreateInspectionRequestDto request)
    {
        var agentIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(agentIdString, out var agentId)) return Unauthorized();

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var result = await _inspectionService.ProcessCheckInAsync(bookingId, agentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Returns all inspections recorded against a booking so the agent can
    /// review prior photos before / after the current check-in or check-out.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory(int bookingId)
    {
        var result = await _inspectionService.GetHistoryForBookingAsync(bookingId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}