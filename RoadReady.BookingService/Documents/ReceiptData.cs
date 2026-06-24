namespace RoadReady.BookingService.Documents;

public record ReceiptItem(string Description, decimal Rate, int Quantity, string Unit, decimal Amount);

public class ReceiptData
{
    public string BookingReference { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string VehicleMakeModel { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime PickupDate { get; set; }
    public DateTime DropoffDate { get; set; }
    public List<ReceiptItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Taxes { get; set; }
    public decimal TotalPaid { get; set; }
}