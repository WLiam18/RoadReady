namespace RoadReady.Shared.DTOs.Admin;
public class AdminAnalyticsDto
{
    public int TotalReservations { get; set; }
    public int ActiveBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal NetRevenue { get; set; }
}