using RoadReady.Shared.Enums;

namespace RoadReady.BookingService.Models;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!; 
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; } 
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending; 
    public string RazorpayPaymentLinkId { get; set; } = string.Empty; 
    public string? RazorpayPaymentId { get; set; } 
    public string PaymentUrl { get; set; } = string.Empty;
    public string? RazorpayRefundId { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}