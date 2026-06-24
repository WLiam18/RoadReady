using RoadReady.Shared.DTOs.Auth;
using RoadReady.Shared.Responses;

namespace RoadReady.AuthService.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<AuthResponseDto>> RegisterAgentAsync(RegisterRequestDto request); // Added this line
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<AuthResponseDto>> LoginWithGoogleAsync(string credential);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ApiResponse<string>> UpdatePasswordAsync(Guid userId, UpdatePasswordRequestDto request);
    Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
    Task<ApiResponse<UserDto>> GetProfileAsync(Guid userId);
}