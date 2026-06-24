using RoadReady.Shared.Enums;

namespace RoadReady.Shared.DTOs.Booking;

public class BookingDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int CarId { get; set; }
    public string CarMake { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string CarImageUrl { get; set; } = string.Empty;
    public DateTime PickupDate { get; set; }
    public DateTime DropoffDate { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public bool IncludesCarSeat { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    
    // NEW: Expose these to the response so you can click the link!
    public string PaymentUrl { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}