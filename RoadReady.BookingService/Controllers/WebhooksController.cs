using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using RoadReady.BookingService.Data;
using RoadReady.Shared.Enums;
using RoadReady.BookingService.Documents; 
using QuestPDF.Fluent; 

namespace RoadReady.BookingService.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly BookingDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhooksController> _logger;
    private readonly IHttpClientFactory _httpClientFactory; 

    public WebhooksController(
        BookingDbContext context, 
        IConfiguration configuration, 
        ILogger<WebhooksController> logger,
        IHttpClientFactory httpClientFactory) 
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory; 
    }

    [HttpPost("razorpay")]
    [AllowAnonymous]
    public async Task<IActionResult> RazorpayWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var jsonBody = await reader.ReadToEndAsync();

        var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
        var webhookSecret = _configuration["Razorpay:WebhookSecret"];

        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(webhookSecret))
        {
            return BadRequest("Missing Signature or Secret.");
        }

        try
        {
            Utils.verifyWebhookSignature(jsonBody, signature, webhookSecret);

            using var doc = JsonDocument.Parse(jsonBody);
            var root = doc.RootElement;
            var eventName = root.GetProperty("event").GetString();

            if (eventName == "payment_link.paid")
            {
                var referenceId = root
                    .GetProperty("payload")
                    .GetProperty("payment_link")
                    .GetProperty("entity")
                    .GetProperty("reference_id")
                    .GetString();

                if (!string.IsNullOrEmpty(referenceId) && referenceId.StartsWith("BOOKING_"))
                {
                    var bookingIdString = referenceId.Replace("BOOKING_", "");
                    if (int.TryParse(bookingIdString, out int bookingId))
                    {
                        var booking = await _context.Bookings
                            .Include(b => b.Payments) 
                            .FirstOrDefaultAsync(b => b.Id == bookingId);
                        
                        if (booking != null && booking.Status == BookingStatus.PendingPayment)
                        {
                            var payment = booking.Payments.FirstOrDefault(p => p.Type == PaymentType.InitialCharge);
                            if (payment != null)
                            {
                                payment.Status = PaymentStatus.Succeeded;
                            }
                            booking.Status = BookingStatus.Confirmed;
                            booking.UpdatedAt = DateTime.UtcNow;

                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Webhook SUCCESS: Booking {BookingId} is now Confirmed and Payment Succeeded.", bookingId);

                            try 
                            {
                                var client = _httpClientFactory.CreateClient();
                                
                                var carServiceUrl = $"http://localhost:5002/api/v1/cars/{booking.CarId}";
                                var carResponse = await client.GetAsync(carServiceUrl);
                                string carName = "Unknown Vehicle";

                                if (carResponse.IsSuccessStatusCode)
                                {
                                    var carJson = await carResponse.Content.ReadAsStringAsync();
                                    using var carDoc = JsonDocument.Parse(carJson);
                                    var carRoot = carDoc.RootElement;
                                    
                                    if (carRoot.TryGetProperty("data", out var dataElement) || carRoot.TryGetProperty("Data", out dataElement))
                                    {
                                        if (dataElement.ValueKind == JsonValueKind.Object) carRoot = dataElement;
                                    }

                                    string make = carRoot.TryGetProperty("make", out var makeProp) || carRoot.TryGetProperty("Make", out makeProp) ? makeProp.GetString() ?? "" : "";
                                    string model = carRoot.TryGetProperty("model", out var modelProp) || carRoot.TryGetProperty("Model", out modelProp) ? modelProp.GetString() ?? "" : "";
                                    if (!string.IsNullOrEmpty(make) || !string.IsNullOrEmpty(model)) carName = $"{make} {model}".Trim();
                                }
                                else 
                                {
                                    _logger.LogWarning("Could not fetch Car Details for CarId {CarId}. Car Service returned {StatusCode}.", booking.CarId, carResponse.StatusCode);
                                }

                                var authServiceUrl = $"http://localhost:5001/api/v1/auth/users/{booking.UserId}";
                                var userResponse = await client.GetAsync(authServiceUrl);
                                
                                string customerName = "Valued Customer";
                                string customerEmail = "N/A";
                                string customerPhone = "N/A";

                                if (userResponse.IsSuccessStatusCode)
                                {
                                    var userJson = await userResponse.Content.ReadAsStringAsync();
                                    using var userDoc = JsonDocument.Parse(userJson);
                                    var userRoot = userDoc.RootElement;
                                    
                                    if (userRoot.TryGetProperty("data", out var userDataElement) || userRoot.TryGetProperty("Data", out userDataElement))
                                    {
                                        if (userDataElement.ValueKind == JsonValueKind.Object) userRoot = userDataElement;
                                    }

                                    string fName = userRoot.TryGetProperty("firstName", out var fn) || userRoot.TryGetProperty("FirstName", out fn) ? fn.GetString() ?? "" : "";
                                    string lName = userRoot.TryGetProperty("lastName", out var ln) || userRoot.TryGetProperty("LastName", out ln) ? ln.GetString() ?? "" : "";
                                    
                                    if (!string.IsNullOrEmpty(fName) || !string.IsNullOrEmpty(lName))
                                    {
                                        customerName = $"{fName} {lName}".Trim();
                                    }
                                        
                                    if (userRoot.TryGetProperty("email", out var emailProp) || userRoot.TryGetProperty("Email", out emailProp))
                                        customerEmail = emailProp.GetString() ?? customerEmail;
                                        
                                    if (userRoot.TryGetProperty("phoneNumber", out var phoneProp) || userRoot.TryGetProperty("PhoneNumber", out phoneProp))
                                        customerPhone = phoneProp.GetString() ?? customerPhone;
                                }
                                else
                                {
                                    _logger.LogWarning("Could not fetch User Details for UserId {UserId}. User Service returned {StatusCode}.", booking.UserId, userResponse.StatusCode);
                                }

                                decimal totalPaid = booking.TotalAmount;
                                decimal subtotal = Math.Round(totalPaid / 1.18m, 2);
                                decimal taxes = totalPaid - subtotal;
                                int rentalDays = (booking.DropoffDate - booking.PickupDate).Days;
                                if (rentalDays == 0) rentalDays = 1;

                                var receiptData = new ReceiptData
                                {
                                    BookingReference = $"RR-BKG-{booking.Id}",
                                    IssueDate = DateTime.UtcNow,
                                    CustomerName = customerName,
                                    CustomerEmail = customerEmail,
                                    CustomerPhone = customerPhone,
                                    VehicleMakeModel = carName,
                                    Location = booking.PickupLocation ?? "Main Lot",
                                    PickupDate = booking.PickupDate,
                                    DropoffDate = booking.DropoffDate,
                                    Subtotal = subtotal,
                                    Taxes = taxes,
                                    TotalPaid = totalPaid,
                                    Items = new List<ReceiptItem>
                                    {
                                        new ReceiptItem("Vehicle Base Rate", Math.Round(subtotal / rentalDays, 2), rentalDays, "day", subtotal)
                                    }
                                };

                                if (booking.IncludesCarSeat)
                                {
                                    receiptData.Items.Add(new ReceiptItem("Child Car Seat Add-on", 500.00m, rentalDays, "day", 500.00m * rentalDays));
                                }

                                var document = new BookingReceiptDocument(receiptData);
                                byte[] pdfBytes = document.GeneratePdf();

                                System.IO.File.WriteAllBytes($"Receipt_BKG_{booking.Id}.pdf", pdfBytes);

                                _logger.LogInformation("Beautiful Receipt PDF generated successfully for Booking {BookingId}.", bookingId);
                            }
                            catch (Exception pdfEx)
                            {
                                _logger.LogError(pdfEx, "Failed to generate PDF receipt for Booking {BookingId}.", bookingId);
                            }
                        }
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay Webhook failed verification or processing.");
            return BadRequest();
        }
    }
}