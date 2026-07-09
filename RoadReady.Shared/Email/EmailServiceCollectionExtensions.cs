using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RoadReady.Shared.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddRoadReadyEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = (configuration["Email:Provider"] ?? "Brevo").Trim();

        if (string.Equals(provider, "Brevo", StringComparison.OrdinalIgnoreCase))
        {
            var apiKey = configuration["Brevo:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                services.AddSingleton<IEmailService, NullEmailService>();
                return services;
            }
            services.AddHttpClient<IEmailService, BrevoEmailService>();
            return services;
        }

        var resendKey = configuration["Resend:ApiKey"];
        if (string.Equals(provider, "Resend", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(resendKey))
        {
            services.AddHttpClient<IEmailService, ResendEmailService>();
            return services;
        }

        services.AddSingleton<IEmailService, NullEmailService>();
        return services;
    }
}

internal sealed class NullEmailService : IEmailService
{
    private readonly ILogger<NullEmailService> _logger;

    public NullEmailService(ILogger<NullEmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendPasswordResetLinkAsync(string toEmail, string toName, string resetLink, string token, DateTime expiresAt)
    {
        _logger.LogWarning("[Stub email] Password reset link for {Email}: {Link} (expires {Expiry:u})", toEmail, resetLink, expiresAt);
        return Task.FromResult(false);
    }

    public Task<bool> SendBookingConfirmationAsync(string toEmail, string toName, int bookingId, string carMakeModel, DateTime pickupDate, DateTime dropoffDate, decimal totalAmount, string paymentUrl)
    {
        _logger.LogWarning("[Stub email] Booking confirmation for #{BookingId} -> {Email}: pay {Url}", bookingId, toEmail, paymentUrl);
        return Task.FromResult(false);
    }

    public Task<bool> SendPaymentReceiptAsync(string toEmail, string toName, int bookingId, string carMakeModel, decimal amount, string receiptUrl)
    {
        _logger.LogWarning("[Stub email] Receipt for #{BookingId} -> {Email}: {Receipt}", bookingId, toEmail, receiptUrl);
        return Task.FromResult(false);
    }

    public Task<bool> SendBookingCancellationAsync(string toEmail, string toName, int bookingId, string carMakeModel, decimal refundAmount)
    {
        _logger.LogWarning("[Stub email] Cancellation for #{BookingId} -> {Email}, refund {Refund}", bookingId, toEmail, refundAmount);
        return Task.FromResult(false);
    }

    public Task<bool> SendWelcomeAsync(string toEmail, string toName)
    {
        _logger.LogWarning("[Stub email] Welcome to {Email}", toEmail);
        return Task.FromResult(false);
    }

    public Task<bool> SendCheckOutConfirmationAsync(string toEmail, string toName, int bookingId, string carMakeModel)
    {
        _logger.LogWarning("[Stub email] Check-out confirmation for #{BookingId} -> {Email}", bookingId, toEmail);
        return Task.FromResult(false);
    }

    public Task<bool> SendCheckInCompletionAsync(string toEmail, string toName, int bookingId, string carMakeModel)
    {
        _logger.LogWarning("[Stub email] Check-in / review-request for #{BookingId} -> {Email}", bookingId, toEmail);
        return Task.FromResult(false);
    }
}
