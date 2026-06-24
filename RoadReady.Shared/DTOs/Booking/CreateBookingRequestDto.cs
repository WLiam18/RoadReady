namespace RoadReady.Shared.DTOs.Booking;

public class CreateBookingRequestDto
{
    public int CarId { get; set; }
    public DateTime PickupDate { get; set; }
    public DateTime DropoffDate { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public bool IncludesCarSeat { get; set; }
}
