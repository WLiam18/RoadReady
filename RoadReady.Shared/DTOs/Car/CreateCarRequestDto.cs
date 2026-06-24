namespace RoadReady.Shared.DTOs.Car;

public class CreateCarRequestDto
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public string Transmission { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public int BrandId { get; set; }
}
