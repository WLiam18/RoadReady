using Microsoft.AspNetCore.Http;

namespace RoadReady.Shared.DTOs.Booking;

public class CreateInspectionRequestDto
{
    public int OdometerReading { get; set; }
    public string FuelLevel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<IFormFile>? VehicleImages { get; set; }
    public List<IFormFile>? DocumentImages { get; set; }
}