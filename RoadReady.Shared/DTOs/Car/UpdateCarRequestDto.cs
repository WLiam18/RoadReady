using RoadReady.Shared.Enums;

namespace RoadReady.Shared.DTOs.Car;

public class UpdateCarRequestDto
{
    public string Location { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public CarStatus Status { get; set; }
}
