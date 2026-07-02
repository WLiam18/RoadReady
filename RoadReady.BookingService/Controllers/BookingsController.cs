using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadReady.BookingService.Interfaces;
using RoadReady.BookingService.Data;
using Microsoft.EntityFrameworkCore;
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
    private readonly IFileStorageService _fileStorage;

    public BookingsController(IBookingService bookingService, IFileStorageService fileStorage)
    {
        _bookingService = bookingService;
        _fileStorage = fileStorage;
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

    [HttpGet("me/payments")]
    public async Task<IActionResult> GetMyPayments()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));
        }

        var result = await _bookingService.GetPaymentsByUserIdAsync(userId.Value);
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

    [HttpGet("unavailable-car-ids")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetUnavailableCarIds([FromQuery] DateTime pickupDate, [FromQuery] DateTime dropoffDate)
    {
        var carIds = await _bookingService.GetUnavailableCarIdsAsync(pickupDate, dropoffDate);
        return Ok(carIds);
    }

    [HttpGet("{id:int}/receipt")]
    public async Task<IActionResult> GetReceipt(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));

        var booking = await _bookingService.GetByIdForReceiptAsync(id);
        if (booking == null) return NotFound(ApiResponse<string>.Fail("Booking not found."));

        var isAdminOrAgent = User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.RentalAgent));
        if (!isAdminOrAgent && booking.UserId != userId.Value)
            return Forbid();

        var initialPayment = booking.Payments?.FirstOrDefault(p => p.Type == PaymentType.InitialCharge && p.Status == PaymentStatus.Succeeded);
        if (initialPayment == null || string.IsNullOrEmpty(initialPayment.ReceiptUrl))
            return NotFound(ApiResponse<string>.Fail("Receipt not available for this booking yet. Please complete payment first."));

        var bytes = await _fileStorage.ReadFileAsync(initialPayment.ReceiptUrl);
        if (bytes == null) return NotFound(ApiResponse<string>.Fail("Receipt file is missing on the server."));

        return File(bytes, "application/pdf", $"RR-BKG-{id}.pdf");
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }
}