namespace RoadReady.Shared.DTOs.Booking;

public class ModifyBookingRequestDto
{
    public DateTime PickupDate { get; set; }
    public DateTime DropoffDate { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public bool IncludesCarSeat { get; set; }
}
