using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadReady.BookingService.Interfaces;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.Enums;
using RoadReady.Shared.Responses;

namespace RoadReady.BookingService.Controllers;

[ApiController]
[Route("api/v1/bookings")]
[Authorize] 
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));
        }

        var result = await _bookingService.CreateAsync(userId.Value, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return StatusCode(201, result);
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.RentalAgent))]
    public async Task<IActionResult> GetAll()
    {
        var result = await _bookingService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));
        }

        var isAdminOrAgent = User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.RentalAgent));
        var result = await _bookingService.GetByIdAsync(id, userId.Value, isAdminOrAgent);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));
        }

        var result = await _bookingService.GetByUserIdAsync(userId.Value);
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.RentalAgent))]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _bookingService.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));
        }

        var isAdminOrAgent = User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.RentalAgent));
        
        var result = await _bookingService.CancelAsync(id, userId.Value, isAdminOrAgent);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:int}/modify")]
    public async Task<IActionResult> Modify(int id, [FromBody] ModifyBookingRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));
        }

        var isAdminOrAgent = User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.RentalAgent));
        
        var result = await _bookingService.ModifyAsync(id, request, userId.Value, isAdminOrAgent);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("verify-eligibility")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> VerifyEligibility([FromQuery] int carId, [FromQuery] Guid userId)
    {
        var result = await _bookingService.VerifyReviewEligibilityAsync(carId, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }
}