using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadReady.CarService.Interfaces;
using RoadReady.Shared.DTOs.Car;
using System.Security.Claims;

namespace RoadReady.CarService.Controllers;

[ApiController]
[Route("api/v1/cars/{carId}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReview(int carId, [FromBody] CreateReviewRequestDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { Success = false, Message = "Invalid user token." });
        }

        var result = await _reviewService.AddReviewAsync(carId, userId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetReviews(int carId)
    {
        var result = await _reviewService.GetReviewsByCarIdAsync(carId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("{reviewId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateReview(int carId, Guid reviewId, [FromBody] UpdateReviewRequestDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { Success = false, Message = "Invalid user token." });
        }

        var result = await _reviewService.UpdateReviewAsync(reviewId, userId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int carId, Guid reviewId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { Success = false, Message = "Invalid user token." });
        }

        var result = await _reviewService.DeleteReviewAsync(reviewId, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}