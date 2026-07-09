using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RoadReady.BookingService.Data;
using RoadReady.BookingService.Interfaces;
using RoadReady.BookingService.Models;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Email;
using RoadReady.Shared.Enums;
using RoadReady.Shared.Responses;
using Microsoft.AspNetCore.Http;

namespace RoadReady.BookingService.Implementations;

public class InspectionService : IInspectionService
{
    private readonly BookingDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<InspectionService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    // Deployment timezone for legacy Unspecified DateTimes in the DB.
    private const int IST_OFFSET_MINUTES = 330;

    public InspectionService(
        BookingDbContext context,
        IFileStorageService fileStorage,
        ILogger<InspectionService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _context = context;
        _fileStorage = fileStorage;
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
        _httpClient = httpClientFactory.CreateClient();
    }

    // ---------- formatting / timezone helpers ----------

    // Use ISO 8601 (yyyy-MM-dd HH:mm) regardless of server culture so error
    // messages never get misread (7/8/2026 vs 8/7/2026 ambiguity is gone).
    private static string Fmt(DateTime dt) =>
        dt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    // Treat Unspecified-kind DateTimes as the deployment timezone (IST) for
    // any time-zone-sensitive comparison against `DateTime.UtcNow`.
    private static DateTime AsIstAware(DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc).AddMinutes(-IST_OFFSET_MINUTES);
        }
        return dt.Kind == DateTimeKind.Local ? dt.ToUniversalTime() : dt;
    }

    // ---------- check-out ----------

    public async Task<ApiResponse<string>> ProcessCheckOutAsync(int bookingId, Guid agentId, CreateInspectionRequestDto request)
    {
        var booking = await _context.Bookings
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null) return ApiResponse<string>.Fail("Booking not found.");

        if (booking.Status != BookingStatus.Confirmed)
            return ApiResponse<string>.Fail("Only confirmed bookings can be checked out.");

        var now = DateTime.UtcNow;
        var pickupUtc = AsIstAware(booking.PickupDate);
        var earliestAllowedCheckOut = pickupUtc.AddHours(-2);

        if (now < earliestAllowedCheckOut)
        {
            return ApiResponse<string>.Fail(
                $"Too early. Check-out opens 2 hours before pickup ({Fmt(earliestAllowedCheckOut)} UTC).");
        }

        if (now > AsIstAware(booking.DropoffDate))
        {
            return ApiResponse<string>.Fail("Booking has expired. Cannot check out after the drop-off date.");
        }

        try
        {
            var vehicleImagePaths = await _fileStorage.SaveFilesAsync(request.VehicleImages ?? new(), "vehicle_images");
            var documentImagePaths = await _fileStorage.SaveFilesAsync(request.DocumentImages ?? new(), "kyc_documents");

            var inspection = new VehicleInspection
            {
                BookingId = bookingId,
                AgentId = agentId,
                Type = InspectionType.CheckOut,
                OdometerReading = request.OdometerReading,
                FuelLevel = request.FuelLevel,
                Notes = request.Notes ?? string.Empty,
                VehicleImagePaths = string.Join(",", vehicleImagePaths),
                DocumentImagePaths = string.Join(",", documentImagePaths),
                CreatedAt = DateTime.UtcNow
            };

            await _context.VehicleInspections.AddAsync(inspection);

            booking.Status = BookingStatus.Active;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Fire-and-log: send confirmation email to customer
            _ = TryEmailCustomerOnCheckOutAsync(booking, agentId);

            return ApiResponse<string>.Ok("Check-out processed successfully.", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process check-out for booking {BookingId}", bookingId);
            return ApiResponse<string>.Fail("An error occurred during check-out.");
        }
    }

    // ---------- check-in ----------

    public async Task<ApiResponse<string>> ProcessCheckInAsync(int bookingId, Guid agentId, CreateInspectionRequestDto request)
    {
        var booking = await _context.Bookings
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null) return ApiResponse<string>.Fail("Booking not found.");

        if (booking.Status != BookingStatus.Active)
            return ApiResponse<string>.Fail("Only active bookings can be checked in.");

        if (DateTime.UtcNow < AsIstAware(booking.PickupDate).AddHours(-2))
        {
            return ApiResponse<string>.Fail(
                $"Invalid return time. This booking has not officially started (pickup: {Fmt(AsIstAware(booking.PickupDate))} UTC).");
        }

        try
        {
            var vehicleImagePaths = await _fileStorage.SaveFilesAsync(request.VehicleImages ?? new(), "vehicle_images");

            var inspection = new VehicleInspection
            {
                BookingId = bookingId,
                AgentId = agentId,
                Type = InspectionType.CheckIn,
                OdometerReading = request.OdometerReading,
                FuelLevel = request.FuelLevel,
                Notes = request.Notes ?? string.Empty,
                VehicleImagePaths = string.Join(",", vehicleImagePaths),
                DocumentImagePaths = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _context.VehicleInspections.AddAsync(inspection);

            booking.Status = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Fire-and-log: send thank-you + review-request email to customer
            _ = TryEmailCustomerOnCheckInAsync(booking, agentId);

            return ApiResponse<string>.Ok("Check-in processed successfully.", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process check-in for booking {BookingId}", bookingId);
            return ApiResponse<string>.Fail("An error occurred during check-in.");
        }
    }

    // ---------- history (for the agent portal) ----------

    public async Task<ApiResponse<BookingInspectionSummaryDto>> GetHistoryForBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return ApiResponse<BookingInspectionSummaryDto>.Fail("Booking not found.");
        }

        var inspections = await _context.VehicleInspections
            .Where(x => x.BookingId == bookingId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        // Customer info — fetched from AuthService over HTTP (we share JWTs through gateway)
        var (customerId, customerName, customerEmail, customerPhone) =
            await FetchUserInfoAsync(booking.UserId);

        var (carMake, carModel, carImage) = await FetchCarInfoAsync(booking.CarId);

        var agentLookup = inspections.Select(i => i.AgentId).Distinct().ToList();
        var agentNames = await FetchAgentsAsync(agentLookup);

        var inspectionIds = inspections.Select(i => i.AgentId).Distinct().ToList();

        var dto = new BookingInspectionSummaryDto
        {
            BookingStatus = booking.Status.ToString(),
            Inspections = inspections.Select(i => new InspectionDto
            {
                Id = i.Id,
                BookingId = i.BookingId,
                Type = i.Type.ToString(),
                OdometerReading = i.OdometerReading,
                FuelLevel = i.FuelLevel,
                Notes = i.Notes,
                VehicleImageUrls = MakeAbsolutePaths(SplitPaths(i.VehicleImagePaths)),
                DocumentImageUrls = MakeAbsolutePaths(SplitPaths(i.DocumentImagePaths)),
                CreatedAt = i.CreatedAt,

                CustomerId = booking.UserId,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,

                AgentId = i.AgentId,
                AgentName = i.AgentId == Guid.Empty ? "(unknown)" : agentNames.GetValueOrDefault(i.AgentId, "(unknown)"),
                AgentEmail = "",

                PickupDate = booking.PickupDate,
                DropoffDate = booking.DropoffDate,
                PickupLocation = booking.PickupLocation ?? string.Empty,
                CarId = booking.CarId,
                CarMake = carMake,
                CarModel = carModel,
                CarImageUrl = carImage,
            }).ToList()
        };

        return ApiResponse<BookingInspectionSummaryDto>.Ok(dto, "Inspection history fetched.");
    }

    // ---------- helpers ----------

    private static IEnumerable<string> SplitPaths(string csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    // Convert "vehicle_images/abc.jpg" to a fetchable URL.
    // If the path is already a full URL (http://...) leave it. Otherwise, since
    // InspectionService is on port 5003 and the gateway serves /uploads via
    // routes.httpclient, build a gateway URL.
    private List<string> MakeAbsolutePaths(IEnumerable<string> paths)
    {
        var outList = new List<string>();
        if (paths == null) return outList;
        var gateway = _configuration["Services:GatewayBaseUrl"]?.TrimEnd('/')
                      ?? (_configuration["Services:AuthServiceBaseUrl"] is string authBase
                          ? authBase.Replace(":5001", ":5000") : "http://localhost:5000");
        foreach (var p in paths)
        {
            if (string.IsNullOrWhiteSpace(p)) continue;
            if (p.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                outList.Add(p);
            }
            else
            {
                // Stored paths from LocalFileStorageService look like "/uploads/vehicle_images/abc.jpg".
                // Ocelot routes GET /uploads/{everything} → 5003. So we just stamp the gateway
                // base in front and strip any leading slash duplication.
                var withLeading = p.StartsWith("/") ? p : "/" + p;
                outList.Add($"{gateway}{withLeading}");
            }
        }
        return outList;
    }

    private class UserInfo { public string Email { get; set; } = ""; public string FirstName { get; set; } = ""; public string LastName { get; set; } = ""; public string PhoneNumber { get; set; } = ""; }

    private async Task<(Guid Id, string Name, string Email, string Phone)> FetchUserInfoAsync(Guid userId)
    {
        try
        {
            var authBase = _configuration["Services:AuthServiceBaseUrl"] ?? "http://localhost:5001";
            var url = $"{authBase.TrimEnd('/')}/api/v1/auth/users/{userId}";
            // Token only needed if endpoint is [Authorize]. AuthService GetById requires authorize.
            // For the agent/admin use case, send an admin token? Keep it lazy — fall back to "".
            using var resp = await _httpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
                return (userId, "", "", "");
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            var data = root.TryGetProperty("data", out var d) ? d : root;
            var firstName = data.TryGetProperty("firstName", out var fn) ? fn.GetString() ?? "" :
                            data.TryGetProperty("FirstName", out fn) ? fn.GetString() ?? "" : "";
            var lastName = data.TryGetProperty("lastName", out var ln) ? ln.GetString() ?? "" :
                           data.TryGetProperty("LastName", out ln) ? ln.GetString() ?? "" : "";
            var email = data.TryGetProperty("email", out var em) ? em.GetString() ?? "" :
                        data.TryGetProperty("Email", out em) ? em.GetString() ?? "" : "";
            var phone = data.TryGetProperty("phoneNumber", out var ph) ? ph.GetString() ?? "" :
                        data.TryGetProperty("PhoneNumber", out ph) ? ph.GetString() ?? "" : "";
            return (userId, $"{firstName} {lastName}".Trim(), email, phone);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch customer info for {UserId}", userId);
            return (userId, "", "", "");
        }
    }

    private async Task<(string Make, string Model, string Image)> FetchCarInfoAsync(int carId)
    {
        try
        {
            var carBase = _configuration["Services:CarServiceBaseUrl"] ?? "http://localhost:5002";
            var url = $"{carBase.TrimEnd('/')}/api/v1/cars/{carId}";
            using var resp = await _httpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return ("", "", "");
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            var data = root.TryGetProperty("data", out var d) ? d : root;
            var make = data.TryGetProperty("make", out var mk) ? mk.GetString() ?? "" :
                       data.TryGetProperty("Make", out mk) ? mk.GetString() ?? "" : "";
            var model = data.TryGetProperty("model", out var md) ? md.GetString() ?? "" :
                        data.TryGetProperty("Model", out md) ? md.GetString() ?? "" : "";
            var image = "";
            if (data.TryGetProperty("imageUrls", out var iu) && iu.ValueKind == JsonValueKind.Array && iu.GetArrayLength() > 0)
            {
                image = iu[0].GetString() ?? "";
            }
            else if (data.TryGetProperty("ImageUrls", out iu) && iu.ValueKind == JsonValueKind.Array && iu.GetArrayLength() > 0)
            {
                image = iu[0].GetString() ?? "";
            }
            return (make, model, image);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch car info for {CarId}", carId);
            return ("", "", "");
        }
    }

    private async Task<Dictionary<Guid, string>> FetchAgentsAsync(List<Guid> agentIds)
    {
        var dict = new Dictionary<Guid, string>();
        if (agentIds.Count == 0) return dict;
        foreach (var id in agentIds)
        {
            try
            {
                var (_, name, _, _) = await FetchUserInfoAsync(id);
                if (!string.IsNullOrWhiteSpace(name)) dict[id] = name;
            }
            catch { /* ignore */ }
        }
        return dict;
    }

    // ---------- customer emails ----------

    private async Task TryEmailCustomerOnCheckOutAsync(Booking booking, Guid agentId)
    {
        try
        {
            var (_, customerName, customerEmail, _) = await FetchUserInfoAsync(booking.UserId);
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                _logger.LogWarning("Skipping check-out email: no email for booking #{Id}.", booking.Id);
                return;
            }
            var (make, model, _) = await FetchCarInfoAsync(booking.CarId);
            await _emailService.SendCheckOutConfirmationAsync(
                customerEmail,
                string.IsNullOrWhiteSpace(customerName) ? customerEmail : customerName,
                booking.Id,
                $"{make} {model}".Trim());
            _logger.LogInformation("Sent check-out confirmation email to {Email} for booking #{Id}.", customerEmail, booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send check-out email for booking #{Id}.", booking.Id);
        }
    }

    private async Task TryEmailCustomerOnCheckInAsync(Booking booking, Guid agentId)
    {
        try
        {
            var (_, customerName, customerEmail, _) = await FetchUserInfoAsync(booking.UserId);
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                _logger.LogWarning("Skipping check-in email: no email for booking #{Id}.", booking.Id);
                return;
            }
            var (make, model, _) = await FetchCarInfoAsync(booking.CarId);
            await _emailService.SendCheckInCompletionAsync(
                customerEmail,
                string.IsNullOrWhiteSpace(customerName) ? customerEmail : customerName,
                booking.Id,
                $"{make} {model}".Trim());
            _logger.LogInformation("Sent check-in thank-you email to {Email} for booking #{Id}.", customerEmail, booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send check-in email for booking #{Id}.", booking.Id);
        }
    }
}
