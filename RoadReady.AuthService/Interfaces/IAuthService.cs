using RoadReady.Shared.DTOs.Auth;
using RoadReady.Shared.Responses;

namespace RoadReady.AuthService.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<AuthResponseDto>> RegisterAgentAsync(RegisterRequestDto request);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<AuthResponseDto>> LoginWithGoogleAsync(string credential);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<ApiResponse<string>> UpdatePasswordAsync(Guid userId, UpdatePasswordRequestDto request);
    Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
    Task<ApiResponse<UserDto>> GetProfileAsync(Guid userId);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<string>> LogoutAsync(Guid userId);
    Task<ApiResponse<List<UserDto>>> GetAllUsersAsync();
    Task<ApiResponse<UserDto>> SetUserActiveStatusAsync(Guid userId, bool isActive);
}