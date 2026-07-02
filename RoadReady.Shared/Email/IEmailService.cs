namespace RoadReady.Shared.Email;

public interface IEmailService
{
    Task<bool> SendPasswordResetLinkAsync(string toEmail, string toName, string resetLink, string token, DateTime expiresAt);
    Task<bool> SendBookingConfirmationAsync(string toEmail, string toName, int bookingId, string carMakeModel, DateTime pickupDate, DateTime dropoffDate, decimal totalAmount, string paymentUrl);
    Task<bool> SendPaymentReceiptAsync(string toEmail, string toName, int bookingId, string carMakeModel, decimal amount, string receiptUrl);
    Task<bool> SendBookingCancellationAsync(string toEmail, string toName, int bookingId, string carMakeModel, decimal refundAmount);
    Task<bool> SendWelcomeAsync(string toEmail, string toName);
}
