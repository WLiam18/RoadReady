using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RoadReady.BookingService.Interfaces;

namespace RoadReady.BookingService.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize] 
public class AdminController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public AdminController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> GetAnalytics()
    {
        bool isAdmin = User.IsInRole("Admin"); 

        var response = await _bookingService.GetAdminAnalyticsAsync(isAdmin);

        if (response.Success)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }
}