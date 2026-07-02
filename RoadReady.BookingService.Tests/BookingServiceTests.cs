using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using RoadReady.BookingService.Implementations;
using RoadReady.BookingService.Interfaces;
using RoadReady.BookingService.Models;
using RoadReady.Shared.DTOs.Admin;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Email;
using RoadReady.Shared.Enums;
using RoadReady.Shared.Responses;

namespace RoadReady.BookingService.Tests;

[TestFixture]
public class BookingServiceTests
{
    private Mock<IBookingRepository> _mockRepo;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private Mock<ILogger<Implementations.BookingService>> _mockLogger;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IEmailService> _mockEmailService;
    
    private HttpClient _httpClient;
    private Implementations.BookingService _bookingService;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IBookingRepository>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<Implementations.BookingService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockEmailService = new Mock<IEmailService>();

        _mockConfiguration.Setup(c => c["Razorpay:KeyId"]).Returns("test_key_id");
        _mockConfiguration.Setup(c => c["Razorpay:KeySecret"]).Returns("test_key_secret");

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5002/")
        };

        _mockEmailService.Setup(e => e.SendBookingConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<string>()))
                         .ReturnsAsync(true);
        _mockEmailService.Setup(e => e.SendBookingCancellationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>()))
                         .ReturnsAsync(true);

        _bookingService = new Implementations.BookingService(
            _mockRepo.Object,
            _httpClient,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockEmailService.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [TestCase(0)]
    [TestCase(-2)]
    public async Task CreateAsync_WhenDatesAreInvalid_ReturnsFail(int daysToAdd)
    {
        var pickupDate = new DateTime(2026, 7, 1);

        var request = new CreateBookingRequestDto
        {
            CarId = 1,
            PickupDate = pickupDate,
            DropoffDate = daysToAdd == 0 ? pickupDate : pickupDate.AddDays(daysToAdd)
        };

        var result = await _bookingService.CreateAsync(Guid.NewGuid(), request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Drop-off date must be after pick-up date."));
        _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Test]
    public async Task CreateAsync_WhenCarHasOverlap_ReturnsFail()
    {
        var request = new CreateBookingRequestDto
        {
            CarId = 1,
            PickupDate = DateTime.UtcNow.AddDays(1),
            DropoffDate = DateTime.UtcNow.AddDays(3)
        };

        _mockRepo.Setup(r => r.HasOverlappingBookingAsync(request.CarId, request.PickupDate, request.DropoffDate, null))
            .ReturnsAsync(true);

        var result = await _bookingService.CreateAsync(Guid.NewGuid(), request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("This car is already booked for the selected dates."));
    }

    [Test]
    public async Task CreateAsync_WithValidData_SavesBookingButFailsRazorpayGracefully()
    {
        var userId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow; 
        
        var request = new CreateBookingRequestDto
        {
            CarId = 1,
            PickupDate = baseDate.AddDays(1),
            DropoffDate = baseDate.AddDays(4),
            PickupLocation = "Chennai Airport",
            IncludesCarSeat = true
        };

        var dummyCar = new CarDto
        {
            Id = 1,
            Make = "Toyota",
            Model = "Innova",
            PricePerDay = 2500,
            ImageUrls = new List<string> { "img.jpg" }
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(ApiResponse<CarDto>.Ok(dummyCar))
            });

        _mockRepo.Setup(repo => repo.HasOverlappingBookingAsync(
            It.IsAny<int>(), 
            It.IsAny<DateTime>(), 
            It.IsAny<DateTime>(),
            It.IsAny<int?>()))
        .ReturnsAsync(false);

        var result = await _bookingService.CreateAsync(userId, request);

        _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<Booking>()), Times.Once);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("payment link generation failed"));
    }

    [Test]
    public async Task CancelAsync_WhenUserIsNotOwner_ReturnsFail()
    {
        var bookingId = 1;
        var ownerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid(); 
        
        var existingBooking = new Booking
        {
            Id = bookingId,
            UserId = ownerId,
            Status = BookingStatus.Confirmed
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(bookingId))
                 .ReturnsAsync(existingBooking);

        var result = await _bookingService.CancelAsync(bookingId, requestingUserId, isAdmin: false);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Booking not found.")); 
        Assert.That(existingBooking.Status, Is.EqualTo(BookingStatus.Confirmed)); 
    }

    [Test]
    public async Task CancelAsync_WhenUserIsAdmin_BypassesOwnerCheckAndCancels()
    {
        var bookingId = 1;
        var ownerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        
        var existingBooking = new Booking
        {
            Id = bookingId,
            UserId = ownerId,
            Status = BookingStatus.Confirmed,
            Payments = new List<Payment>()
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(bookingId))
                 .ReturnsAsync(existingBooking);

        var result = await _bookingService.CancelAsync(bookingId, adminId, isAdmin: true);

        Assert.That(result.Success, Is.True);
        Assert.That(existingBooking.Status, Is.EqualTo(BookingStatus.Cancelled)); 
        _mockRepo.Verify(repo => repo.UpdateAsync(existingBooking), Times.Once);
    }

    [Test]
    public async Task GetAdminAnalyticsAsync_WhenNotAdmin_ReturnsFail()
    {
        var result = await _bookingService.GetAdminAnalyticsAsync(isAdmin: false);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Unauthorized access"));
    }

    [Test]
    public async Task GetAdminAnalyticsAsync_WhenAdmin_ReturnsAnalytics()
    {
        var dummyAnalytics = new AdminAnalyticsDto
        {
            TotalReservations = 15,
            ActiveBookings = 10,
            TotalRevenue = 15000
        };

        _mockRepo.Setup(r => r.GetAnalyticsAsync()).ReturnsAsync(dummyAnalytics);

        var result = await _bookingService.GetAdminAnalyticsAsync(isAdmin: true);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.TotalReservations, Is.EqualTo(15));
        Assert.That(result.Data.TotalRevenue, Is.EqualTo(15000));
    }
}