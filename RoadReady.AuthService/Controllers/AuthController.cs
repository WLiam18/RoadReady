using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadReady.AuthService.Interfaces;
using RoadReady.Shared.DTOs.Auth;
using RoadReady.Shared.Responses;
using System.Security.Claims;

namespace RoadReady.AuthService.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success) return BadRequest(result);
        return StatusCode(201, result);
    }

    [HttpPost("register-agent")]
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> RegisterAgent([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAgentAsync(request);
        if (!result.Success) return BadRequest(result);
        return StatusCode(201, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await _authService.LoginWithGoogleAsync(request.Credential);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("update-password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));

        var result = await _authService.UpdatePasswordAsync(userId.Value, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));

        var result = await _authService.UpdateProfileAsync(userId.Value, request);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));

        var result = await _authService.GetProfileAsync(userId.Value);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(ApiResponse<string>.Fail("Unauthorized user."));

        var result = await _authService.LogoutAsync(userId.Value);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("users/{userId:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequestDto request)
    {
        var result = await _authService.SetUserActiveStatusAsync(userId, request.IsActive);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    //using like this for testing have to change when frontend is built
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserByIdForMicroservice(Guid id)
    {
        var result = await _authService.GetProfileAsync(id);
        if (!result.Success) return NotFound(result);
        
        return Ok(result);
    }
    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }
}

public class GoogleLoginRequest
{
    public string Credential { get; set; } = string.Empty;
}