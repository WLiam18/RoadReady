using BCrypt.Net;
using Google.Apis.Auth;
using RoadReady.AuthService.Interfaces;
using RoadReady.AuthService.Models;
using RoadReady.Shared.DTOs.Auth;
using RoadReady.Shared.Enums;
using RoadReady.Shared.Responses;

namespace RoadReady.AuthService.Implementations;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        IAuthRepository authRepository,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _authRepository = authRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _configuration = configuration;
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

        var accessToken = _jwtTokenService.GenerateToken(user);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = "not-implemented",
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };

        return ApiResponse<AuthResponseDto>.Created(response, "User registered successfully.");
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
            Role = UserRole.RentalAgent, // Securely hardcoded to RentalAgent
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

        var accessToken = _jwtTokenService.GenerateToken(user);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = "not-implemented",
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };

        return ApiResponse<AuthResponseDto>.Created(response, "Rental Agent registered successfully.");
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

        var accessToken = _jwtTokenService.GenerateToken(user);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = "not-implemented",
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };

        return ApiResponse<AuthResponseDto>.Ok(response, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginWithGoogleAsync(string credential)
    {
        var clientId = _configuration["Google:ClientId"];

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

        var accessToken = _jwtTokenService.GenerateToken(user);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = "not-implemented",
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };

        return ApiResponse<AuthResponseDto>.Ok(response, "Google login successful.");
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        var user = await _authRepository.GetByEmailAsync(normalizedEmail);

        if (user == null)
        {
            return ApiResponse<string>.Fail("No account found with this email.");
        }

        _logger.LogInformation("Password reset requested for email: {Email}", user.Email);

        return ApiResponse<string>.Ok("Password reset link would be sent here.", "Password reset request submitted.");
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

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<UserDto>.Ok(userDto, "Profile updated successfully.");
    }

    public async Task<ApiResponse<UserDto>> GetProfileAsync(Guid userId)
    {
        var user = await _authRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("User not found.");
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<UserDto>.Ok(userDto, "Profile fetched successfully.");
    }
}