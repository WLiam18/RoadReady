namespace RoadReady.Shared.Email;

public static class EmailTemplates
{
    private const string BrandPrimary = "#FF6B35";
    private const string BrandSecondary = "#1E3A5F";
    private const string BrandBg = "#F7F8FA";
    private const string BrandBody = "#2D3748";
    private const string BrandMuted = "#718096";

    private static string Layout(string title, string body) => $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin:0;padding:0;background:{BrandBg};font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;'>
<table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background:{BrandBg};'>
  <tr>
    <td align='center' style='padding:32px 16px;'>
      <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='max-width:600px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);'>
        <tr>
          <td style='background:{BrandSecondary};padding:24px 32px;text-align:center;'>
            <h1 style='margin:0;color:#ffffff;font-size:24px;letter-spacing:-0.5px;'>&#128665; RoadReady</h1>
            <p style='margin:4px 0 0;color:#cbd5e0;font-size:13px;'>Premium car rentals made simple</p>
          </td>
        </tr>
        <tr>
          <td style='padding:32px;'>
            <h2 style='margin:0 0 16px;color:{BrandBody};font-size:20px;'>{title}</h2>
            {body}
          </td>
        </tr>
        <tr>
          <td style='background:#f7fafc;padding:20px 32px;text-align:center;color:{BrandMuted};font-size:12px;'>
            <p style='margin:0;'>&copy; {DateTime.UtcNow.Year} RoadReady. Drive with confidence.</p>
            <p style='margin:8px 0 0;'>Need help? Reply to this email or visit our support center.</p>
          </td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</body>
</html>";

    private static string Button(string url, string text, string color = BrandPrimary) =>
        $@"<table role='presentation' cellspacing='0' cellpadding='0' border='0' style='margin:24px 0;'>
            <tr>
              <td style='background:{color};border-radius:8px;text-align:center;'>
                <a href='{url}' style='display:inline-block;padding:14px 32px;color:#ffffff;text-decoration:none;font-weight:600;font-size:15px;'>{text}</a>
              </td>
            </tr>
          </table>";

    public static string PasswordReset(string toName, string resetLink, DateTime expiresAt) => Layout(
        "Reset your password",
        $@"<p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>Hi <strong>{toName}</strong>,</p>
           <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>We received a request to reset the password for your RoadReady account. Click the button below to choose a new password. This link expires at <strong>{expiresAt:HH:mm} UTC</strong> on <strong>{expiresAt:dddd, MMMM d}</strong>.</p>
           {Button(resetLink, "Reset My Password")}
           <p style='margin:0 0 16px;color:{BrandMuted};font-size:13px;line-height:1.6;'>If the button doesn't work, copy and paste this link into your browser:<br/><a href='{resetLink}' style='color:{BrandPrimary};word-break:break-all;'>{resetLink}</a></p>
           <p style='margin:16px 0 0;color:{BrandMuted};font-size:13px;line-height:1.6;'>Didn't request a password reset? You can safely ignore this email &mdash; your password will stay the same.</p>"
    );

    public static string BookingConfirmation(string toName, int bookingId, string carMakeModel, DateTime pickupDate, DateTime dropoffDate, decimal totalAmount, string paymentUrl) => Layout(
        $"Booking #{bookingId} created",
        $@"<p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>Hi <strong>{toName}</strong>,</p>
           <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>Your booking has been created. To <strong>confirm and lock in</strong> your reservation, please complete payment using the secure link below.</p>
           <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background:#f7fafc;border-radius:8px;margin:16px 0;'>
             <tr><td style='padding:16px;'>
               <p style='margin:0 0 8px;color:{BrandMuted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Booking Reference</p>
               <p style='margin:0 0 16px;color:{BrandBody};font-size:18px;font-weight:700;'>RR-BKG-{bookingId}</p>
               <p style='margin:0 0 8px;color:{BrandMuted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Vehicle</p>
               <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;'>{carMakeModel}</p>
               <p style='margin:0 0 8px;color:{BrandMuted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Pick-up</p>
               <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;'>{pickupDate:dddd, MMMM d, yyyy 'at' HH:mm}</p>
               <p style='margin:0 0 8px;color:{BrandMuted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Drop-off</p>
               <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;'>{dropoffDate:dddd, MMMM d, yyyy 'at' HH:mm}</p>
               <p style='margin:0 0 8px;color:{BrandMuted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Total Amount</p>
               <p style='margin:0;color:{BrandPrimary};font-size:22px;font-weight:700;'>&#8377;{totalAmount:N0}</p>
             </td></tr>
           </table>
           {Button(paymentUrl, "Complete Payment Securely")}
           <p style='margin:16px 0 0;color:{BrandMuted};font-size:13px;line-height:1.6;'>This link is valid for 24 hours. After payment, you'll receive a PDF receipt instantly.</p>"
    );

    public static string PaymentReceipt(string toName, int bookingId, string carMakeModel, decimal amount, string receiptUrl) => Layout(
        $"Payment received for booking #{bookingId}",
        $@"<p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>Hi <strong>{toName}</strong>,</p>
           <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>We've received your payment of <strong>&#8377;{amount:N0}</strong> for booking <strong>#{bookingId}</strong> ({carMakeModel}). Your reservation is now confirmed.</p>
           {Button(receiptUrl, "Download PDF Receipt")}
           <p style='margin:16px 0 0;color:{BrandMuted};font-size:13px;line-height:1.6;'>Please carry a valid driver's license and a photo ID at the time of pick-up. Your rental agent will perform a quick inspection before handing over the keys.</p>
           <p style='margin:16px 0 0;color:{BrandMuted};font-size:13px;line-height:1.6;'>Drive safe and enjoy the ride!</p>"
    );

    public static string BookingCancellation(string toName, int bookingId, string carMakeModel, decimal refundAmount) => Layout(
        $"Booking #{bookingId} cancelled",
        $@"<p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>Hi <strong>{toName}</strong>,</p>
           <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>Your booking <strong>#{bookingId}</strong> for <strong>{carMakeModel}</strong> has been cancelled.</p>
           <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background:#f7fafc;border-radius:8px;margin:16px 0;'>
             <tr><td style='padding:16px;'>
               <p style='margin:0 0 8px;color:{BrandMuted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Refund Amount</p>
               <p style='margin:0;color:{BrandPrimary};font-size:22px;font-weight:700;'>&#8377;{refundAmount:N0}</p>
             </td></tr>
           </table>
           <p style='margin:0;color:{BrandBody};font-size:15px;line-height:1.6;'>Any refund due has been initiated and should reflect in your account within <strong>5-7 business days</strong>.</p>
           <p style='margin:16px 0 0;color:{BrandMuted};font-size:13px;line-height:1.6;'>We hope to see you again soon!</p>"
    );

    public static string Welcome(string toName) => Layout(
        "Welcome to RoadReady",
        $@"<p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>Hi <strong>{toName}</strong>,</p>
           <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>Welcome aboard! Your RoadReady account is ready. Browse our fleet of premium cars, pick your dates, and book in seconds.</p>
           <p style='margin:0 0 16px;color:{BrandBody};font-size:15px;line-height:1.6;'>We use industry-standard encryption to keep your data safe and we never store your full payment details.</p>
           <p style='margin:16px 0 0;color:{BrandMuted};font-size:13px;line-height:1.6;'>If you ever need help, just hit reply &mdash; we're always one message away.</p>
           <p style='margin:24px 0 0;color:{BrandBody};font-size:15px;'>Cheers,<br/><strong>The RoadReady Team</strong></p>"
    );
}
