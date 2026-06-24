namespace RoadReady.Shared.DTOs.Car;

public class CarSearchRequestDto
{
    public string? Location { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? DropoffDate { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public int? SeatingCapacity { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
