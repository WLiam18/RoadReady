using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RoadReady.Shared.Email;

public sealed class ResendEmailService : IEmailService
{
    private const string ResendApiUrl = "https://api.resend.com/emails";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;

    public ResendEmailService(HttpClient httpClient, ILogger<ResendEmailService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;

        _apiKey = configuration["Resend:ApiKey"]
            ?? Environment.GetEnvironmentVariable("RESEND_API_KEY")
            ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _httpClient.BaseAddress = new Uri("https://api.resend.com/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }

    private string FromAddress => _configuration["Email:FromAddress"] ?? "no-reply@roadready.app";
    private string FromName => _configuration["Email:FromName"] ?? "RoadReady";

    private async Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("[Email stub] RESEND_API_KEY missing. Would send to {To}: {Subject}", toEmail, subject);
            return false;
        }

        try
        {
            var payload = new ResendPayload
            {
                From = $"{FromName} <{FromAddress}>",
                To = new List<string> { $"{(string.IsNullOrWhiteSpace(toName) ? toEmail : toName)} <{toEmail}>" },
                Subject = subject,
                Html = htmlBody
            };

            var response = await _httpClient.PostAsJsonAsync("emails", payload);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Resend API error {Status}: {Body}", response.StatusCode, body);
                return false;
            }

            _logger.LogInformation("Email sent to {To}: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", toEmail);
            return false;
        }
    }

    public Task<bool> SendPasswordResetLinkAsync(string toEmail, string toName, string resetLink, string token, DateTime expiresAt) =>
        SendAsync(toEmail, toName, "Reset your RoadReady password", EmailTemplates.PasswordReset(toName, resetLink, expiresAt));

    public Task<bool> SendBookingConfirmationAsync(string toEmail, string toName, int bookingId, string carMakeModel, DateTime pickupDate, DateTime dropoffDate, decimal totalAmount, string paymentUrl) =>
        SendAsync(toEmail, toName, $"Booking #{bookingId} created - complete payment to confirm",
            EmailTemplates.BookingConfirmation(toName, bookingId, carMakeModel, pickupDate, dropoffDate, totalAmount, paymentUrl));

    public Task<bool> SendPaymentReceiptAsync(string toEmail, string toName, int bookingId, string carMakeModel, decimal amount, string receiptUrl) =>
        SendAsync(toEmail, toName, $"Payment receipt for booking #{bookingId}",
            EmailTemplates.PaymentReceipt(toName, bookingId, carMakeModel, amount, receiptUrl));

    public Task<bool> SendBookingCancellationAsync(string toEmail, string toName, int bookingId, string carMakeModel, decimal refundAmount) =>
        SendAsync(toEmail, toName, $"Booking #{bookingId} cancelled",
            EmailTemplates.BookingCancellation(toName, bookingId, carMakeModel, refundAmount));

    public Task<bool> SendWelcomeAsync(string toEmail, string toName) =>
        SendAsync(toEmail, toName, "Welcome to RoadReady", EmailTemplates.Welcome(toName));

    private sealed class ResendPayload
    {
        [JsonPropertyName("from")] public string From { get; set; } = "";
        [JsonPropertyName("to")] public List<string> To { get; set; } = new();
        [JsonPropertyName("subject")] public string Subject { get; set; } = "";
        [JsonPropertyName("html")] public string Html { get; set; } = "";
    }
}
