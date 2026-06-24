namespace RoadReady.Shared.DTOs.Car;

public class CreateReviewRequestDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}