using BCrypt.Net;
using Google.Apis.Auth;
using RoadReady.AuthService.Interfaces;
using RoadReady.AuthService.Models;
using RoadReady.Shared.DTOs.Auth;
using RoadReady.Shared.Email;
using RoadReady.Shared.Enums;
using RoadReady.Shared.Responses;

namespace RoadReady.AuthService.Implementations;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(
        IAuthRepository authRepository,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _authRepository = authRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        var existingUser = await _authRepository.GetByEmailAsync(normalizedEmail);

        if (existingUser != null)
        {
            return ApiResponse<AuthResponseDto>.Fail("An account with this email already exists.");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            AuthProvider = "Local",
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _authRepository.AddUserAsync(user);
        await _authRepository.SaveChangesAsync();

        _logger.LogInformation("User registered successfully with email: {Email}", user.Email);

        var tokens = await GenerateTokensAsync(user);

        await _emailService.SendWelcomeAsync(user.Email, $"{user.FirstName} {user.LastName}".Trim());

        return ApiResponse<AuthResponseDto>.Created(tokens, "User registered successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAgentAsync(RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        var existingUser = await _authRepository.GetByEmailAsync(normalizedEmail);

        if (existingUser != null)
        {
            return ApiResponse<AuthResponseDto>.Fail("An account with this email already exists.");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            AuthProvider = "Local",
            Role = UserRole.RentalAgent,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _authRepository.AddUserAsync(user);
            await _authRepository.SaveChangesAsync();
            _logger.LogInformation("Rental Agent registered successfully with email: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error occurred while registering Rental Agent: {Email}", user.Email);
            return ApiResponse<AuthResponseDto>.Fail("An error occurred while creating the agent account.");
        }

        var tokens = await GenerateTokensAsync(user);

        await _emailService.SendWelcomeAsync(user.Email, $"{user.FirstName} {user.LastName}".Trim());

        return ApiResponse<AuthResponseDto>.Created(tokens, "Rental Agent registered successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        var user = await _authRepository.GetByEmailAsync(normalizedEmail);

        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponseDto>.Fail("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponseDto>.Fail("Your account is inactive. Please contact admin.");
        }

        _logger.LogInformation("User logged in successfully with email: {Email}", user.Email);

        var tokens = await GenerateTokensAsync(user);

        return ApiResponse<AuthResponseDto>.Ok(tokens, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginWithGoogleAsync(string credential)
    {
        var clientId = _configuration["Authentication:Google:ClientId"] ?? _configuration["Google:ClientId"];

        if (string.IsNullOrWhiteSpace(clientId))
        {
            return ApiResponse<AuthResponseDto>.Fail("Google authentication is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(credential, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });
        }
        catch
        {
            return ApiResponse<AuthResponseDto>.Fail("Invalid Google credential.");
        }

        var normalizedEmail = payload.Email.Trim().ToLower();
        var user = await _authRepository.GetByEmailAsync(normalizedEmail);

        if (user == null)
        {
            user = new User
            {
                FirstName = payload.GivenName ?? string.Empty,
                LastName = payload.FamilyName ?? string.Empty,
                Email = normalizedEmail,
                PhoneNumber = string.Empty,
                PasswordHash = null,
                GoogleId = payload.Subject,
                AuthProvider = "Google",
                ProfileImageUrl = payload.Picture,
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.AddUserAsync(user);
        }
        else
        {
            user.GoogleId ??= payload.Subject;
            user.AuthProvider = "Google";
            user.ProfileImageUrl = payload.Picture ?? user.ProfileImageUrl;

            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                user.FirstName = payload.GivenName ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                user.LastName = payload.FamilyName ?? string.Empty;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _authRepository.UpdateUserAsync(user);
        }

        await _authRepository.SaveChangesAsync();

        var tokens = await GenerateTokensAsync(user);

        return ApiResponse<AuthResponseDto>.Ok(tokens, "Google login successful.");
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        var user = await _authRepository.GetByEmailAsync(normalizedEmail);

        if (user == null)
        {
            return ApiResponse<string>.Ok("If an account with that email exists, a password reset link has been sent.", "Password reset request submitted.");
        }

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _authRepository.AddPasswordResetTokenAsync(resetToken);
        await _authRepository.SaveChangesAsync();

        var frontendBaseUrl = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?token={resetToken.Token}&email={Uri.EscapeDataString(user.Email)}";

        _logger.LogInformation("Password reset requested for email: {Email}. Reset link: {Link}", user.Email, resetLink);

        await _emailService.SendPasswordResetLinkAsync(
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            resetLink,
            resetToken.Token,
            resetToken.ExpiresAt);

        return ApiResponse<string>.Ok("If an account with that email exists, a password reset link has been sent.", "Password reset request submitted.");
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var storedToken = await _authRepository.GetPasswordResetTokenAsync(request.Token);

        if (storedToken == null || storedToken.IsUsed || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return ApiResponse<string>.Fail("Invalid or expired reset token.");
        }

        if (storedToken.User == null)
        {
            return ApiResponse<string>.Fail("User account not found.");
        }

        storedToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        storedToken.User.UpdatedAt = DateTime.UtcNow;
        storedToken.IsUsed = true;

        await _authRepository.RevokeRefreshTokensForUserAsync(storedToken.UserId);
        await _authRepository.UpdateUserAsync(storedToken.User);
        await _authRepository.SaveChangesAsync();

        _logger.LogInformation("Password reset successfully for UserId: {UserId}", storedToken.UserId);

        return ApiResponse<string>.Ok("Password reset successfully. Please log in with your new password.");
    }

    public async Task<ApiResponse<string>> UpdatePasswordAsync(Guid userId, UpdatePasswordRequestDto request)
    {
        var user = await _authRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return ApiResponse<string>.Fail("User not found.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return ApiResponse<string>.Fail("Password update is not available for this account.");
        }

        var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

        if (!isCurrentPasswordValid)
        {
            return ApiResponse<string>.Fail("Current password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.UpdateUserAsync(user);
        await _authRepository.SaveChangesAsync();

        _logger.LogInformation("Password updated successfully for user: {Email}", user.Email);

        return ApiResponse<string>.Ok("Password updated successfully.");
    }

    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
    {
        var user = await _authRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("User not found.");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.UpdateUserAsync(user);
        await _authRepository.SaveChangesAsync();

        _logger.LogInformation("Profile updated successfully for user: {Email}", user.Email);

        return ApiResponse<UserDto>.Ok(MapUserToDto(user), "Profile updated successfully.");
    }

    public async Task<ApiResponse<UserDto>> GetProfileAsync(Guid userId)
    {
        var user = await _authRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("User not found.");
        }

        return ApiResponse<UserDto>.Ok(MapUserToDto(user), "Profile fetched successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _authRepository.GetRefreshTokenAsync(refreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return ApiResponse<AuthResponseDto>.Fail("Invalid or expired refresh token.");
        }

        if (storedToken.User == null || !storedToken.User.IsActive)
        {
            return ApiResponse<AuthResponseDto>.Fail("User account is not active.");
        }

        storedToken.IsRevoked = true;
        var newTokens = await GenerateTokensAsync(storedToken.User);

        _logger.LogInformation("Token refreshed for UserId: {UserId}", storedToken.UserId);

        return ApiResponse<AuthResponseDto>.Ok(newTokens, "Token refreshed successfully.");
    }

    public async Task<ApiResponse<string>> LogoutAsync(Guid userId)
    {
        await _authRepository.RevokeRefreshTokensForUserAsync(userId);
        await _authRepository.SaveChangesAsync();

        _logger.LogInformation("User logged out, all refresh tokens revoked for UserId: {UserId}", userId);

        return ApiResponse<string>.Ok("Logged out successfully.");
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await _authRepository.GetAllUsersAsync();
        var userDtos = users.Select(MapUserToDto).ToList();
        return ApiResponse<List<UserDto>>.Ok(userDtos, "Users fetched successfully.");
    }

    public async Task<ApiResponse<UserDto>> SetUserActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await _authRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("User not found.");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.UpdateUserAsync(user);

        if (!isActive)
        {
            await _authRepository.RevokeRefreshTokensForUserAsync(userId);
        }

        await _authRepository.SaveChangesAsync();

        _logger.LogInformation("User {UserId} active status set to {IsActive}", userId, isActive);

        return ApiResponse<UserDto>.Ok(MapUserToDto(user), $"User {(isActive ? "activated" : "deactivated")} successfully.");
    }

    private static UserDto MapUserToDto(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        ProfileImageUrl = user.ProfileImageUrl,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };

    private async Task<AuthResponseDto> GenerateTokensAsync(User user)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _authRepository.AddRefreshTokenAsync(refreshTokenEntity);
        await _authRepository.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            User = MapUserToDto(user)
        };
    }
}