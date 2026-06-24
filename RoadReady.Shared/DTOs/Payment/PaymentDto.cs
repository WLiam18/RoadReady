using RoadReady.Shared.Enums;

namespace RoadReady.Shared.DTOs.Payment;

public class PaymentDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "inr";
    public string PaymentMethod { get; set; } = string.Empty;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
