namespace RoadReady.BookingService.Models;

public enum InspectionType
{
    CheckOut,
    CheckIn  
}

public class VehicleInspection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int BookingId { get; set; }
    public Guid AgentId { get; set; }     public InspectionType Type { get; set; } 
    public int OdometerReading { get; set; }
    public string FuelLevel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty; 
    public string VehicleImagePaths { get; set; } = string.Empty; 
    public string DocumentImagePaths { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = null!;
}