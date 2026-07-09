using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RoadReady.Shared.Email;

public sealed class BrevoEmailService : IEmailService
{
    private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

    private readonly HttpClient _httpClient;
    private readonly ILogger<BrevoEmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public BrevoEmailService(HttpClient httpClient, ILogger<BrevoEmailService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;

        _apiKey = configuration["Brevo:ApiKey"] ?? string.Empty;
        _senderEmail = configuration["Brevo:SenderEmail"] ?? configuration["Email:FromAddress"] ?? "no-reply@roadready.app";
        _senderName = configuration["Email:FromName"] ?? "RoadReady";

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _httpClient.BaseAddress = new Uri("https://api.brevo.com/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }
    }

    private string FromAddress => _senderEmail;
    private string FromName => _senderName;

    private async Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("[Email stub] Brevo API key missing. Would send to {To}: {Subject}", toEmail, subject);
            return false;
        }

        try
        {
            var payload = new BrevoPayload
            {
                Sender = new BrevoContact { Name = FromName, Email = FromAddress },
                To = new List<BrevoContact> { new BrevoContact { Email = toEmail, Name = string.IsNullOrWhiteSpace(toName) ? toEmail : toName } },
                Subject = subject,
                HtmlContent = htmlBody
            };

            var response = await _httpClient.PostAsJsonAsync("v3/smtp/email", payload);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo API error {Status}: {Body}", response.StatusCode, body);
                return false;
            }

            _logger.LogInformation("Email sent via Brevo to {To}: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Brevo to {To}", toEmail);
            return false;
        }
    }

    public Task<bool> SendPasswordResetLinkAsync(string toEmail, string toName, string resetLink, string token, DateTime expiresAt)
    {
        return SendAsync(toEmail, toName, "Reset your RoadReady password", EmailTemplates.PasswordReset(toName, resetLink, expiresAt));
    }

    public Task<bool> SendBookingConfirmationAsync(string toEmail, string toName, int bookingId, string carMakeModel, DateTime pickupDate, DateTime dropoffDate, decimal totalAmount, string paymentUrl)
    {
        return SendAsync(toEmail, toName, $"Booking #{bookingId} created - complete payment to confirm",
            EmailTemplates.BookingConfirmation(toName, bookingId, carMakeModel, pickupDate, dropoffDate, totalAmount, paymentUrl));
    }

    public Task<bool> SendPaymentReceiptAsync(string toEmail, string toName, int bookingId, string carMakeModel, decimal amount, string receiptUrl)
    {
        return SendAsync(toEmail, toName, $"Payment receipt for booking #{bookingId}",
            EmailTemplates.PaymentReceipt(toName, bookingId, carMakeModel, amount, receiptUrl));
    }

    public Task<bool> SendBookingCancellationAsync(string toEmail, string toName, int bookingId, string carMakeModel, decimal refundAmount)
    {
        return SendAsync(toEmail, toName, $"Booking #{bookingId} cancelled",
            EmailTemplates.BookingCancellation(toName, bookingId, carMakeModel, refundAmount));
    }

    public Task<bool> SendWelcomeAsync(string toEmail, string toName)
    {
        return SendAsync(toEmail, toName, "Welcome to RoadReady", EmailTemplates.Welcome(toName));
    }

    public Task<bool> SendCheckOutConfirmationAsync(string toEmail, string toName, int bookingId, string carMakeModel)
    {
        return SendAsync(toEmail, toName, $"Your vehicle is ready - drive safely", EmailTemplates.CheckOutConfirmation(toName, bookingId, carMakeModel));
    }

    public Task<bool> SendCheckInCompletionAsync(string toEmail, string toName, int bookingId, string carMakeModel)
    {
        var frontendBase = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000";
        var reviewUrl = $"{frontendBase.TrimEnd('/')}/cars/{bookingId}#reviews";
        var html = EmailTemplates.CheckInCompletion(toName, bookingId, carMakeModel)
                                 .Replace("{{REVIEW_URL}}", System.Net.WebUtility.HtmlEncode(reviewUrl));
        return SendAsync(toEmail, toName, $"Thanks for choosing RoadReady - Booking #{bookingId}", html);
    }

    private sealed class BrevoPayload
    {
        [JsonPropertyName("sender")] public BrevoContact Sender { get; set; } = new();
        [JsonPropertyName("to")] public List<BrevoContact> To { get; set; } = new();
        [JsonPropertyName("subject")] public string Subject { get; set; } = "";
        [JsonPropertyName("htmlContent")] public string HtmlContent { get; set; } = "";
    }

    private sealed class BrevoContact
    {
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
    }
}
