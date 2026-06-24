namespace RoadReady.CarService.Models;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int CarId { get; set; } 
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Car Car { get; set; } = null!;
}