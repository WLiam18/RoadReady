using RoadReady.Shared.Enums;

namespace RoadReady.BookingService.Models;

public class Booking
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
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}