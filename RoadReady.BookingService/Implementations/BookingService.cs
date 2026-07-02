using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using RoadReady.BookingService.Interfaces;
using RoadReady.BookingService.Models;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.DTOs.Admin;
using RoadReady.Shared.DTOs.Payment;
using RoadReady.Shared.Email;
using RoadReady.Shared.Enums;
using RoadReady.Shared.Responses;

namespace RoadReady.BookingService.Implementations;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<BookingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public BookingService(
        IBookingRepository bookingRepository,
        HttpClient httpClient,
        ILogger<BookingService> logger,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _bookingRepository = bookingRepository;
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<ApiResponse<BookingDto>> CreateAsync(Guid userId, CreateBookingRequestDto request)
    {
        if (request.PickupDate >= request.DropoffDate)
        {
            return ApiResponse<BookingDto>.Fail("Drop-off date must be after pick-up date.");
        }

        var hasOverlap = await _bookingRepository.HasOverlappingBookingAsync(request.CarId, request.PickupDate, request.DropoffDate);
        if (hasOverlap)
        {
            return ApiResponse<BookingDto>.Fail("This car is already booked for the selected dates.");
        }

        var carResult = await GetCarDetailsAsync(request.CarId);
        if (carResult == null)
        {
            return ApiResponse<BookingDto>.Fail("Selected car does not exist or service is unavailable.");
        }

        var totalAmount = CalculateTotalAmount(
            carResult.PricePerDay,
            request.PickupDate,
            request.DropoffDate,
            request.IncludesCarSeat);

        var pickupDateTime = CombineDateAndTime(request.PickupDate, request.PickupTime);
        var dropoffDateTime = CombineDateAndTime(request.DropoffDate, request.DropoffTime);

        decimal discountAmount = 0m;
        string? appliedPromo = null;
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promoResult = await TryApplyPromoCodeAsync(request.PromoCode, totalAmount);
            if (promoResult.HasValue)
            {
                discountAmount = promoResult.Value.Discount;
                appliedPromo = promoResult.Value.Code;
            }
        }

        var finalAmount = Math.Max(0, totalAmount - discountAmount);

        var booking = new Booking
        {
            UserId = userId,
            CarId = request.CarId,
            CarMake = carResult.Make,
            CarModel = carResult.Model,
            CarImageUrl = carResult.ImageUrls.FirstOrDefault() ?? string.Empty,
            PickupDate = pickupDateTime,
            DropoffDate = dropoffDateTime,
            PickupLocation = request.PickupLocation,
            IncludesCarSeat = request.IncludesCarSeat,
            AppliedPromoCode = appliedPromo,
            Subtotal = totalAmount,
            DiscountAmount = discountAmount,
            TotalAmount = finalAmount,
            Status = BookingStatus.PendingPayment
        };

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveAsync();

        try
        {
            var keyId = _configuration["Razorpay:KeyId"];
            var keySecret = _configuration["Razorpay:KeySecret"];
            
            RazorpayClient client = new RazorpayClient(keyId, keySecret);

            int amountInPaise = (int)(totalAmount * 100);

            Dictionary<string, object> paymentLinkRequest = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", "INR" },
                { "accept_partial", false },
                { "description", $"RoadReady Rental: {booking.CarMake} {booking.CarModel}" },
                { "reference_id", $"BOOKING_{booking.Id}" }, 
                { "reminder_enable", true }
            };

            PaymentLink paymentLink = client.PaymentLink.Create(paymentLinkRequest);

            var payment = new RoadReady.BookingService.Models.Payment
            {
                BookingId = booking.Id,
                Amount = totalAmount,
                Type = PaymentType.InitialCharge,
                Status = PaymentStatus.Pending,
                RazorpayPaymentLinkId = paymentLink.Attributes["id"].ToString(),
                PaymentUrl = paymentLink.Attributes["short_url"].ToString()
            };

            booking.Payments.Add(payment);

            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveAsync();
            
            _logger.LogInformation("Razorpay Link created for BookingId: {BookingId}", booking.Id);

            await SendBookingConfirmationEmailAsync(booking, payment.PaymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Razorpay Payment Link for BookingId: {BookingId}", booking.Id);
            return ApiResponse<BookingDto>.Fail("Booking created, but payment link generation failed. Please try again.");
        }

        return ApiResponse<BookingDto>.Created(MapToDto(booking), "Booking created successfully. Please complete the payment.");
    }

    public async Task<ApiResponse<List<BookingDto>>> GetAllAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        return ApiResponse<List<BookingDto>>.Ok(bookings.Select(MapToDto).ToList(), "Bookings fetched successfully.");
    }

    public async Task<ApiResponse<bool>> VerifyReviewEligibilityAsync(int carId, Guid userId)
    {
    var isEligible = await _bookingRepository.HasCompletedBookingAsync(carId, userId);
    
    if (!isEligible)
    {
        return ApiResponse<bool>.Fail("User is not eligible to review this car.");
    }

    return ApiResponse<bool>.Ok(true, "User is eligible to review.");
    }

    public async Task<List<int>> GetUnavailableCarIdsAsync(DateTime pickupDate, DateTime dropoffDate)
    {
        return await _bookingRepository.GetUnavailableCarIdsAsync(pickupDate, dropoffDate);
    }

    public Task<Booking?> GetByIdForReceiptAsync(int id)
    {
        return _bookingRepository.GetByIdWithPaymentsAsync(id);
    }

    public async Task<ApiResponse<BookingDto>> GetByIdAsync(int id, Guid currentUserId, bool isAdmin)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null || (!isAdmin && booking.UserId != currentUserId))
        {
            return ApiResponse<BookingDto>.Fail("Booking not found.");
        }

        return ApiResponse<BookingDto>.Ok(MapToDto(booking), "Booking fetched successfully.");
    }

    public async Task<ApiResponse<List<BookingDto>>> GetByUserIdAsync(Guid userId)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(userId);
        return ApiResponse<List<BookingDto>>.Ok(bookings.Select(MapToDto).ToList(), "User bookings fetched successfully.");
    }

    public async Task<ApiResponse<List<PaymentDto>>> GetPaymentsByUserIdAsync(Guid userId)
    {
        var payments = await _bookingRepository.GetPaymentsByUserIdAsync(userId);

        var paymentDtos = payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            BookingId = p.BookingId,
            UserId = p.Booking?.UserId ?? Guid.Empty,
            Amount = p.Amount,
            Type = p.Type,
            Status = p.Status,
            PaymentUrl = p.PaymentUrl,
            CreatedAt = p.CreatedAt
        }).ToList();

        return ApiResponse<List<PaymentDto>>.Ok(paymentDtos, "Payment history fetched successfully.");
    }

    public async Task<ApiResponse<BookingDto>> CancelAsync(int id, Guid currentUserId, bool isAdmin)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null || (!isAdmin && booking.UserId != currentUserId))
        {
            return ApiResponse<BookingDto>.Fail("Booking not found.");
        }

        var initialPayment = booking.Payments?.FirstOrDefault(p => 
            p.Type == PaymentType.InitialCharge && 
            p.Status == PaymentStatus.Succeeded);

        if (initialPayment != null && !string.IsNullOrEmpty(initialPayment.RazorpayPaymentId))
        {
            try
            {
                var keyId = _configuration["Razorpay:KeyId"];
                var keySecret = _configuration["Razorpay:KeySecret"];
                RazorpayClient client = new RazorpayClient(keyId, keySecret);

                Dictionary<string, object> refundRequest = new Dictionary<string, object>
                {
                    { "payment_id", initialPayment.RazorpayPaymentId } 
                };

                Refund refund = client.Refund.Create(refundRequest);

                var refundRecord = new RoadReady.BookingService.Models.Payment
                {
                    BookingId = booking.Id,
                    Amount = initialPayment.Amount, 
                    Type = PaymentType.Refund,
                    Status = PaymentStatus.Succeeded,
                    RazorpayRefundId = refund.Attributes["id"].ToString(),
                    CreatedAt = DateTime.UtcNow
                };
                
                booking.Payments ??= new List<RoadReady.BookingService.Models.Payment>();
                booking.Payments.Add(refundRecord);

                _logger.LogInformation("Refund successful for BookingId: {BookingId}", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Razorpay refund for BookingId: {BookingId}", booking.Id);
                return ApiResponse<BookingDto>.Fail("Booking cancelled, but automated refund failed. Please contact support.");
            }
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking);
        await _bookingRepository.SaveAsync();

        await SendBookingCancellationEmailAsync(booking);

        return ApiResponse<BookingDto>.Ok(MapToDto(booking), "Booking cancelled successfully.");
    }

    public async Task<ApiResponse<BookingDto>> ModifyAsync(int id, ModifyBookingRequestDto request, Guid currentUserId, bool isAdmin)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null || (!isAdmin && booking.UserId != currentUserId))
        {
            return ApiResponse<BookingDto>.Fail("Booking not found.");
        }

        if (request.PickupDate >= request.DropoffDate)
        {
            return ApiResponse<BookingDto>.Fail("Drop-off date must be after pick-up date.");
        }

        var hasOverlap = await _bookingRepository.HasOverlappingBookingAsync(booking.CarId, request.PickupDate, request.DropoffDate, booking.Id);
        if (hasOverlap)
        {
            return ApiResponse<BookingDto>.Fail("This car is already booked for the new selected dates.");
        }

        var carResult = await GetCarDetailsAsync(booking.CarId);
        if (carResult == null)
        {
            return ApiResponse<BookingDto>.Fail("Unable to fetch car details for modification.");
        }

        booking.PickupDate = request.PickupDate;
        booking.DropoffDate = request.DropoffDate;
        booking.PickupLocation = request.PickupLocation;
        booking.IncludesCarSeat = request.IncludesCarSeat;
        booking.TotalAmount = CalculateTotalAmount(
            carResult.PricePerDay,
            request.PickupDate,
            request.DropoffDate,
            request.IncludesCarSeat);
        booking.Status = BookingStatus.Modified;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking);
        await _bookingRepository.SaveAsync();

        return ApiResponse<BookingDto>.Ok(MapToDto(booking), "Booking updated successfully.");
    }
    public async Task<ApiResponse<AdminAnalyticsDto>> GetAdminAnalyticsAsync(bool isAdmin)
    {
        if (!isAdmin)
        {
            return ApiResponse<AdminAnalyticsDto>.Fail("Unauthorized access. Admin privileges required.");
        }

        try
        {
            var analytics = await _bookingRepository.GetAnalyticsAsync();
            return ApiResponse<AdminAnalyticsDto>.Ok(analytics, "Analytics retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve admin analytics.");
            return ApiResponse<AdminAnalyticsDto>.Fail("An error occurred while fetching analytics.");
        }
    }

    private async Task<CarDto?> GetCarDetailsAsync(int carId)
    {
        try
        {
            var carResponse = await _httpClient.GetAsync($"api/v1/cars/{carId}");

            if (!carResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Car Service returned {StatusCode} for CarId {CarId}", carResponse.StatusCode, carId);
                return null;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            var carResult = await carResponse.Content.ReadFromJsonAsync<ApiResponse<CarDto>>(jsonOptions);

            return carResult?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while connecting to Car Service for CarId: {CarId}", carId);
            return null; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetCarDetailsAsync for CarId: {CarId}", carId);
            return null;
        }
    }

    private static decimal CalculateTotalAmount(decimal pricePerDay, DateTime pickupDate, DateTime dropoffDate, bool includesCarSeat)
    {
        var totalDays = (dropoffDate.Date - pickupDate.Date).Days;
        if (totalDays < 1) totalDays = 1;

        var extraCharge = includesCarSeat ? 200 : 0;
        return (pricePerDay * totalDays) + extraCharge;
    }

    private static DateTime CombineDateAndTime(DateTime date, string time) => DateTimeCombiner.Combine(date, time);

    private async Task<(string Email, string Name)?> FetchUserInfoAsync(Guid userId)
    {
        try
        {
            var url = $"{_configuration["Services:AuthServiceBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5001"}/api/v1/auth/users/{userId}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var dataElement) ||
                root.TryGetProperty("Data", out dataElement))
            {
                if (dataElement.ValueKind == JsonValueKind.Object) root = dataElement;
            }
            var email = root.TryGetProperty("email", out var e) || root.TryGetProperty("Email", out e)
                ? e.GetString() ?? "" : "";
            if (string.IsNullOrEmpty(email)) return null;

            string fName = root.TryGetProperty("firstName", out var fn) || root.TryGetProperty("FirstName", out fn) ? fn.GetString() ?? "" : "";
            string lName = root.TryGetProperty("lastName", out var ln) || root.TryGetProperty("LastName", out ln) ? ln.GetString() ?? "" : "";
            var name = $"{fName} {lName}".Trim();
            return (email, string.IsNullOrEmpty(name) ? email : name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch user info for UserId: {UserId}", userId);
            return null;
        }
    }

    private async Task SendBookingConfirmationEmailAsync(Booking booking, string paymentUrl)
    {
        var user = await FetchUserInfoAsync(booking.UserId);
        if (user == null) return;

        await _emailService.SendBookingConfirmationAsync(
            user.Value.Email,
            user.Value.Name,
            booking.Id,
            $"{booking.CarMake} {booking.CarModel}".Trim(),
            booking.PickupDate,
            booking.DropoffDate,
            booking.TotalAmount,
            paymentUrl);
    }

    private async Task SendBookingCancellationEmailAsync(Booking booking)
    {
        var user = await FetchUserInfoAsync(booking.UserId);
        if (user == null) return;

        decimal refundAmount = booking.Payments?
            .Where(p => p.Type == PaymentType.Refund && p.Status == PaymentStatus.Succeeded)
            .Sum(p => p.Amount) ?? 0m;

        await _emailService.SendBookingCancellationAsync(
            user.Value.Email,
            user.Value.Name,
            booking.Id,
            $"{booking.CarMake} {booking.CarModel}".Trim(),
            refundAmount);
    }

    private static BookingDto MapToDto(Booking booking)
    {
        var initialPayment = booking.Payments?.FirstOrDefault(p => p.Type == PaymentType.InitialCharge);

        return new BookingDto
        {
            Id = booking.Id,
            UserId = booking.UserId,
            CarId = booking.CarId,
            CarMake = booking.CarMake,
            CarModel = booking.CarModel,
            CarImageUrl = booking.CarImageUrl,
            PickupDate = booking.PickupDate,
            DropoffDate = booking.DropoffDate,
            PickupLocation = booking.PickupLocation,
            IncludesCarSeat = booking.IncludesCarSeat,
            AppliedPromoCode = booking.AppliedPromoCode,
            Subtotal = booking.Subtotal,
            DiscountAmount = booking.DiscountAmount,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            PaymentUrl = initialPayment?.PaymentUrl ?? string.Empty,
            PaymentStatus = initialPayment?.Status.ToString() ?? "Pending",
            ReceiptUrl = initialPayment?.ReceiptUrl,
            CreatedAt = booking.CreatedAt
        };
    }

    private async Task<(string Code, decimal Discount)?> TryApplyPromoCodeAsync(string code, decimal subtotal)
    {
        try
        {
            var promoUrl = $"{_configuration["Services:CarServiceBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5002"}/api/v1/promo-codes/validate";
            var request = new ValidatePromoHttpRequest { Code = code, BookingAmount = subtotal };
            var response = await _httpClient.PostAsJsonAsync(promoUrl, request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Promo code validation returned {StatusCode}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ValidatePromoResponse>>();
            if (result?.Data == null || !result.Data.IsValid)
            {
                _logger.LogInformation("Promo code {Code} invalid: {Message}", code, result?.Data?.Message ?? "unknown");
                return null;
            }

            _logger.LogInformation("Applied promo {Code}. Discount {Discount}", result.Data.Code, result.Data.DiscountAmount);
            return (result.Data.Code, result.Data.DiscountAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate promo code {Code}", code);
            return null;
        }
    }
}

internal record ValidatePromoHttpRequest
{
    public string Code { get; init; } = string.Empty;
    public decimal BookingAmount { get; init; }
}

internal class ValidatePromoResponse
{
    public bool IsValid { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal static class DateTimeCombiner
{
    public static DateTime Combine(DateTime date, string time)
    {
        if (TimeSpan.TryParse(time, out var ts))
        {
            return date.Date + ts;
        }
        return date;
    }
}