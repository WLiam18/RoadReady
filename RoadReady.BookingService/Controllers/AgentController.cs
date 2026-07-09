using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadReady.BookingService.Data;
using RoadReady.BookingService.Interfaces;
using RoadReady.BookingService.Models;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.Enums;
using RoadReady.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace RoadReady.BookingService.Controllers;

/// <summary>
/// Endpoints used by the rental-agent workspace: completed-bookings list,
/// inspection history, etc. Role-gated to RentalAgent and Admin.
/// </summary>
[ApiController]
[Route("api/v1/agent")]
[Authorize(Roles = "RentalAgent,Admin")]
public class AgentController : ControllerBase
{
    private readonly BookingDbContext _context;
    private readonly IInspectionService _inspectionService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        BookingDbContext context,
        IInspectionService inspectionService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AgentController> logger)
    {
        _context = context;
        _inspectionService = inspectionService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// Returns today's bookings that have been checked-in (Completed status),
    /// grouped by booking. Includes the inspection history compiled from
    /// bookingService so the agent sees check-out AND check-in photos paired.
    /// </summary>
    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedForAgent([FromQuery] string? date)
    {
        // Optional day override; default = today (UTC date filter is fine — the
        // user gets all bookings whose day matches). For MVP "today" only.
        DateTime targetDay;
        if (DateTime.TryParse(date, out var parsed))
        {
            targetDay = parsed.Date;
        }
        else
        {
            targetDay = DateTime.UtcNow.Date;
        }
        var dayStart = targetDay;
        var dayEnd = targetDay.AddDays(1).AddTicks(-1);

        var bookings = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Completed
                     && b.UpdatedAt >= dayStart
                     && b.UpdatedAt <= dayEnd)
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync();

        // For each booking, surface the inspection history
        var bookingIds = bookings.Select(b => b.Id).ToList();
        var inspectionMap = await _context.VehicleInspections
            .Where(v => bookingIds.Contains(v.BookingId))
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();

        var response = new List<CompletedBookingDto>();
        foreach (var b in bookings)
        {
            var inspe = inspectionMap.Where(i => i.BookingId == b.Id).ToList();
            var checkout = inspe.FirstOrDefault(i => i.Type == InspectionType.CheckOut);
            var checkin = inspe.FirstOrDefault(i => i.Type == InspectionType.CheckIn);

            var customer = await TryFetchUserAsync(b.UserId);
            var car = await TryFetchCarAsync(b.CarId);
            var agentNames = await TryFetchAgentsAsync(inspe.Select(i => i.AgentId).Distinct().ToList());

            response.Add(new CompletedBookingDto
            {
                Booking = new
                {
                    id = b.Id,
                    pickupDate = b.PickupDate,
                    dropoffDate = b.DropoffDate,
                    pickupLocation = b.PickupLocation,
                    totalAmount = b.TotalAmount,
                    status = b.Status.ToString(),
                    updatedAt = b.UpdatedAt,
                },
                CustomerName = customer.name,
                CustomerEmail = customer.email,
                CustomerPhone = customer.phone,
                CarMake = car.make,
                CarModel = car.model,
                CarImageUrl = car.image,
                CheckOutInspection = MapToDto(checkout, agentNames, customer, car, _configuration),
                CheckInInspection = MapToDto(checkin, agentNames, customer, car, _configuration),
            });
        }

        return Ok(ApiResponse<List<CompletedBookingDto>>.Ok(response, $"Today's completed bookings (count: {response.Count})."));
    }

    private static CompletedInspectionDto? MapToDto(
        Models.VehicleInspection? insp,
        Dictionary<Guid, string> agentNames,
        (Guid id, string name, string email, string phone) customer,
        (string make, string model, string image) car,
        IConfiguration cfg)
    {
        if (insp == null) return null;
        return new CompletedInspectionDto
        {
            Id = insp.Id,
            BookingId = insp.BookingId,
            Type = insp.Type.ToString(),
            OdometerReading = insp.OdometerReading,
            FuelLevel = insp.FuelLevel,
            Notes = insp.Notes,
            VehicleImageUrls = AbsolutizePaths(insp.VehicleImagePaths, cfg),
            DocumentImageUrls = AbsolutizePaths(insp.DocumentImagePaths, cfg),
            AgentId = insp.AgentId,
            AgentName = insp.AgentId == Guid.Empty ? "(unknown)" : agentNames.GetValueOrDefault(insp.AgentId, "(unknown)"),
            CreatedAt = insp.CreatedAt,
            BookingIdRef = insp.BookingId,
            CustomerName = customer.name,
            CustomerEmail = customer.email,
            CustomerPhone = customer.phone,
            CarMake = car.make,
            CarModel = car.model,
        };
    }

    private static List<string> AbsolutizePaths(string csv, IConfiguration cfg)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(csv)) return list;
        var authBase = cfg["Services:AuthServiceBaseUrl"] ?? "http://localhost:5001";
        var gateway = authBase.EndsWith(":5001", StringComparison.OrdinalIgnoreCase)
            ? authBase.Substring(0, authBase.Length - 5) + ":5000"
            : authBase;
        foreach (var raw in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            list.Add(raw.StartsWith("http") ? raw : $"{gateway.TrimEnd('/')}{(raw.StartsWith("/") ? raw : "/" + raw)}");
        }
        return list;
    }

    private async Task<(Guid id, string name, string email, string phone)> TryFetchUserAsync(Guid userId)
    {
        try
        {
            var authBase = _configuration["Services:AuthServiceBaseUrl"] ?? "http://localhost:5001";
            using var resp = await _httpClient.GetAsync($"{authBase.TrimEnd('/')}/api/v1/auth/users/{userId}");
            if (!resp.IsSuccessStatusCode) return (userId, "", "", "");
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            var data = root.TryGetProperty("data", out var d) ? d : root;
            string F(string k1, string k2) => data.TryGetProperty(k1, out var v) ? v.GetString() ?? "" :
                                                  data.TryGetProperty(k2, out v) ? v.GetString() ?? "" : "";
            var fn = F("firstName", "FirstName");
            var ln = F("lastName", "LastName");
            var em = F("email", "Email");
            var ph = F("phoneNumber", "PhoneNumber");
            return (userId, $"{fn} {ln}".Trim(), em, ph);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "user fetch failed {UserId}", userId);
            return (userId, "", "", "");
        }
    }

    private async Task<(string make, string model, string image)> TryFetchCarAsync(int carId)
    {
        try
        {
            var carBase = _configuration["Services:CarServiceBaseUrl"] ?? "http://localhost:5002";
            using var resp = await _httpClient.GetAsync($"{carBase.TrimEnd('/')}/api/v1/cars/{carId}");
            if (!resp.IsSuccessStatusCode) return ("", "", "");
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            var data = root.TryGetProperty("data", out var d) ? d : root;
            string F(string k1, string k2) => data.TryGetProperty(k1, out var v) ? v.GetString() ?? "" :
                                                  data.TryGetProperty(k2, out v) ? v.GetString() ?? "" : "";
            var image = "";
            if (data.TryGetProperty("imageUrls", out var iu) && iu.ValueKind == JsonValueKind.Array && iu.GetArrayLength() > 0)
            {
                image = iu[0].GetString() ?? "";
            }
            else if (data.TryGetProperty("ImageUrls", out iu) && iu.ValueKind == JsonValueKind.Array && iu.GetArrayLength() > 0)
            {
                image = iu[0].GetString() ?? "";
            }
            return (F("make", "Make"), F("model", "Model"), image);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "car fetch failed {CarId}", carId);
            return ("", "", "");
        }
    }

    private async Task<Dictionary<Guid, string>> TryFetchAgentsAsync(List<Guid> ids)
    {
        var map = new Dictionary<Guid, string>();
        foreach (var id in ids)
        {
            var (uid, name, _, _) = await TryFetchUserAsync(id);
            if (!string.IsNullOrWhiteSpace(name)) map[id] = name;
        }
        return map;
    }
}

public class CompletedBookingDto
{
    // Booking summary as anonymous shape (BookingDto already exists; using
    // camelCase anonymous keeps the JSON consistent with the rest of the API).
    public object Booking { get; set; } = new { };
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string CarMake { get; set; } = "";
    public string CarModel { get; set; } = "";
    public string CarImageUrl { get; set; } = "";
    public CompletedInspectionDto? CheckOutInspection { get; set; }
    public CompletedInspectionDto? CheckInInspection { get; set; }
}

public class CompletedInspectionDto
{
    public Guid Id { get; set; }
    public int BookingId { get; set; }
    public int BookingIdRef { get; set; }
    public string Type { get; set; } = "";
    public int OdometerReading { get; set; }
    public string FuelLevel { get; set; } = "";
    public string Notes { get; set; } = "";
    public List<string> VehicleImageUrls { get; set; } = new();
    public List<string> DocumentImageUrls { get; set; } = new();
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string CarMake { get; set; } = "";
    public string CarModel { get; set; } = "";
}
