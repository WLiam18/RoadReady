namespace RoadReady.Shared.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalCars { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
}
