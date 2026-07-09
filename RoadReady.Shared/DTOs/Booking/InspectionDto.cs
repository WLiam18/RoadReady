namespace RoadReady.Shared.DTOs.Booking;

public class InspectionDto
{
    public Guid Id { get; set; }
    public int BookingId { get; set; }
    public string Type { get; set; } = string.Empty;          // "CheckOut" / "CheckIn"
    public int OdometerReading { get; set; }
    public string FuelLevel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<string> VehicleImageUrls { get; set; } = new();
    public List<string> DocumentImageUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    // Customer info (denormalized for display — agent never needs to hit Auth)
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Agent info
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;

    // Booking summary (so the agent page can show what was rented)
    public DateTime PickupDate { get; set; }
    public DateTime DropoffDate { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public int CarId { get; set; }
    public string CarMake { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string CarImageUrl { get; set; } = string.Empty;
}

public class BookingInspectionSummaryDto
{
    public string BookingStatus { get; set; } = string.Empty;
    public List<InspectionDto> Inspections { get; set; } = new();
}
