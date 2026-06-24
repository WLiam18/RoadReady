namespace RoadReady.Shared.DTOs.Payment;

public class CreatePaymentIntentRequestDto
{
    public int BookingId { get; set; }
  
    public string PaymentMethod { get; set; } = string.Empty;
}
