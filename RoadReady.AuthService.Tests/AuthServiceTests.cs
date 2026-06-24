using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RoadReady.AuthService.Implementations;
using RoadReady.AuthService.Interfaces;
using RoadReady.AuthService.Models;
using RoadReady.Shared.DTOs.Auth;
using RoadReady.Shared.Enums;

namespace RoadReady.AuthService.Tests;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IAuthRepository> _mockAuthRepository;
    private Mock<IJwtTokenService> _mockJwtTokenService;
    private Mock<ILogger<Implementations.AuthService>> _mockLogger;
    private Mock<IConfiguration> _mockConfiguration;
    
    private Implementations.AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mockAuthRepository = new Mock<IAuthRepository>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<Implementations.AuthService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        _authService = new Implementations.AuthService(
            _mockAuthRepository.Object,
            _mockJwtTokenService.Object,
            _mockLogger.Object,
            _mockConfiguration.Object
        );
    }

    [Test]
    public async Task RegisterAsync_WithNewEmail_ReturnsCreatedSuccess()
    {
        var request = new RegisterRequestDto
        {
            FirstName = "Test",
            LastName = "User1",
            Email = "new@roadready.com",
            Password = "Password123!",
            PhoneNumber = "1234567890"
        };

        _mockAuthRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                           .ReturnsAsync((User?)null);

        _mockJwtTokenService.Setup(jwt => jwt.GenerateToken(It.IsAny<User>()))
                            .Returns("mocked-jwt-token");

        var result = await _authService.RegisterAsync(request);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("User registered successfully."));
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.AccessToken, Is.EqualTo("mocked-jwt-token"));
        Assert.That(result.Data.User.Email, Is.EqualTo("new@roadready.com"));
        
        _mockAuthRepository.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Once);
        _mockAuthRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFail()
    {
        var request = new RegisterRequestDto 
        { 
            Email = "existing@roadready.com", 
            Password = "Password123!" 
        };
        
        var existingUser = new User { Email = "existing@roadready.com" };

        _mockAuthRepository.Setup(repo => repo.GetByEmailAsync("existing@roadready.com"))
                           .ReturnsAsync(existingUser);

        var result = await _authService.RegisterAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("An account with this email already exists."));
        _mockAuthRepository.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Never);
    }

    [TestCase(UserRole.Customer)]
    [TestCase(UserRole.Admin)]
    [TestCase(UserRole.RentalAgent)]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess(UserRole role)
    {
        var request = new LoginRequestDto
        {
            Email = "valid@roadready.com",
            Password = "Password123!"
        };

        var validUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "valid@roadready.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"), 
            IsActive = true,
            Role = role
        };

        _mockAuthRepository.Setup(repo => repo.GetByEmailAsync("valid@roadready.com"))
                           .ReturnsAsync(validUser);

        _mockJwtTokenService.Setup(jwt => jwt.GenerateToken(validUser))
                            .Returns("mocked-jwt-token");

        var result = await _authService.LoginAsync(request);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.AccessToken, Is.EqualTo("mocked-jwt-token"));
        Assert.That(result.Data!.User.Role, Is.EqualTo(role));    }

    [Test]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsFail()
    {
        var request = new LoginRequestDto
        {
            Email = "ghost@roadready.com",
            Password = "Password123!"
        };

        _mockAuthRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                           .ReturnsAsync((User?)null);

        var result = await _authService.LoginAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid email or password."));
    }

    [Test]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFail()
    {
        var request = new LoginRequestDto
        {
            Email = "valid@roadready.com",
            Password = "WrongPassword!"
        };

        var validUser = new User
        {
            Email = "valid@roadready.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"), 
            IsActive = true
        };

        _mockAuthRepository.Setup(repo => repo.GetByEmailAsync("valid@roadready.com"))
                           .ReturnsAsync(validUser);

        var result = await _authService.LoginAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid email or password."));
    }

    [Test]
    public async Task LoginAsync_WithInactiveAccount_ReturnsFail()
    {
        var request = new LoginRequestDto
        {
            Email = "inactive@roadready.com",
            Password = "Password123!"
        };

        var inactiveUser = new User
        {
            Email = "inactive@roadready.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"), 
            IsActive = false
        };

        _mockAuthRepository.Setup(repo => repo.GetByEmailAsync("inactive@roadready.com"))
                           .ReturnsAsync(inactiveUser);

        var result = await _authService.LoginAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Your account is inactive. Please contact admin."));
    }
}