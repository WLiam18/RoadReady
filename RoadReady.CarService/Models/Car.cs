using RoadReady.Shared.Enums;

namespace RoadReady.CarService.Models;

public class Car
{
    public int Id { get; set; }
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
    public string ImageUrls { get; set; } = string.Empty;
    public CarStatus Status { get; set; } = CarStatus.Available;
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}