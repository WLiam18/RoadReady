namespace RoadReady.Shared.DTOs.Car;

public class ReviewDto
{
    public Guid Id { get; set; }
    public int CarId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserProfileImage { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateReviewRequestDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}