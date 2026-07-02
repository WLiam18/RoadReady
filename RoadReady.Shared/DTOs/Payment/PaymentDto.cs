using RoadReady.Shared.Enums;

namespace RoadReady.Shared.DTOs.Payment;

public class PaymentDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }
    public string? PaymentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
