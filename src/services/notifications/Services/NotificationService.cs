using notifications.Interfaces;
using notifications.Models;

namespace notifications.Services;

public class NotificationService(IEmailService emailService, IConfiguration config) : INotificationService
{
    public async Task SendOrderConfirmedAsync(OrderConfirmedEvent evt)
    {
        var frontendUrl = config["FrontendUrl"] ?? "http://localhost:5173";
        var trackingUrl = $"{frontendUrl}/orders/{evt.OrderNumber}?token={evt.AccessToken}";

        var itemsHtml = string.Join("", evt.Items.Select(i =>
            $"<tr><td style='padding:4px 8px;'>{i.Name}</td><td style='padding:4px 8px;'>x{i.Quantity}</td><td style='padding:4px 8px;'>${i.UnitPrice:F2}/day</td></tr>"));

        var html = $"""
            <div style="font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 24px; color: #3d1f0d;">
              <h1 style="color:#c4953a;">Your Booking is Confirmed!</h1>
              <p>Hi {evt.CustomerName},</p>
              <p>Wonderful news — your Teacup Boutique booking has been confirmed and your payment received.</p>
              <table style="width:100%; border-collapse:collapse; margin:16px 0;">
                <tr><td><strong>Order</strong></td><td>{evt.OrderNumber}</td></tr>
                <tr><td><strong>Event Date</strong></td><td>{evt.ReservationDate:d MMMM yyyy}</td></tr>
                <tr><td><strong>Total Paid</strong></td><td>${evt.Total:F2}</td></tr>
              </table>
              <h3>Your Collection</h3>
              <table style="width:100%; border-collapse:collapse; border:1px solid #e8dcc8;">
                <thead><tr style="background:#f5ede0;">
                  <th style="padding:6px 8px; text-align:left;">Item</th>
                  <th style="padding:6px 8px; text-align:left;">Qty</th>
                  <th style="padding:6px 8px; text-align:left;">Price</th>
                </tr></thead>
                <tbody>{itemsHtml}</tbody>
              </table>
              <p style="margin-top:24px;">
                <a href="{trackingUrl}" style="display:inline-block; background:#c4953a; color:#fff; padding:10px 20px; text-decoration:none; border-radius:4px;">Track Your Order</a>
              </p>
              <p style="margin-top:16px; padding:16px; background:#f5ede0; border-radius:4px; font-size:14px;">
                Want to view your order history anytime?
                <a href="{frontendUrl}/register" style="color:#c4953a;">Create a free account</a> using this email address and all your orders will be linked automatically.
              </p>
              <p style="margin-top:24px; color:#c4953a;">We look forward to making your occasion truly special.</p>
            </div>
            """;

        await emailService.SendAsync(new EmailMessage
        {
            // To = evt.CustomerEmail,
            To = "luke.birchenough@outlook.com",
            ToName = evt.CustomerName,
            Subject = $"Booking Confirmed – {evt.OrderNumber}",
            HtmlBody = html
        });
    }

    public async Task SendPaymentFailedAsync(PaymentFailedEvent evt)
    {
        var html = $"""
            <div style="font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 24px; color: #3d1f0d;">
              <h1>Payment Unsuccessful</h1>
              <p>Hi {evt.CustomerName},</p>
              <p>Unfortunately your payment for order <strong>{evt.OrderNumber}</strong> was unsuccessful (attempt {evt.Attempt} of 3).</p>
              <p>Please try again to secure your booking for <strong>{evt.ReservationDate:d MMMM yyyy}</strong>.</p>
            </div>
            """;

        await emailService.SendAsync(new EmailMessage
        {
            To = evt.CustomerEmail,
            ToName = evt.CustomerName,
            Subject = $"Payment Failed – {evt.OrderNumber}",
            HtmlBody = html
        });
    }

    public async Task SendOrderCancelledAsync(OrderCancelledEvent evt)
    {
        var html = $"""
            <div style="font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 24px; color: #3d1f0d;">
              <h1>Booking Cancelled</h1>
              <p>Hi {evt.CustomerName},</p>
              <p>Your booking <strong>{evt.OrderNumber}</strong> for <strong>{evt.ReservationDate:d MMMM yyyy}</strong> has been cancelled.</p>
              <p>If you'd like to make a new booking, we'd love to hear from you.</p>
            </div>
            """;

        await emailService.SendAsync(new EmailMessage
        {
            //To = evt.CustomerEmail,
            To = "luke.birchenough@outlook.com",
            ToName = evt.CustomerName,
            Subject = $"Booking Cancelled – {evt.OrderNumber}",
            HtmlBody = html
        });
    }

    public async Task SendOrderCompletedAsync(OrderCompletedEvent evt)
    {
        var frontendUrl = config["FrontendUrl"] ?? "http://localhost:5173";
        var trackingUrl = $"{frontendUrl}/orders/{evt.OrderNumber}?token={evt.AccessToken}";

        var depositHtml = evt.DepositAmountKept switch
        {
            null => "",
            0 => "<p>Your full deposit will be returned to you.</p>",
            _ => $"<p>A deposit amount of <strong>${evt.DepositAmountKept:F2}</strong> has been retained. Please see your order details and return summary.</p>"
        };

        var notesHtml = !string.IsNullOrEmpty(evt.CompletionNotes)
            ? $"<p><strong>Notes from us:</strong> {evt.CompletionNotes}</p>"
            : "";

        var html = $"""
            <div style="font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 24px; color: #3d1f0d;">
              <h1 style="color:#c4953a;">Your Booking is Complete</h1>
              <p>Hi {evt.CustomerName},</p>
              <p>Thank you for your booking. Your items have been returned and your booking for <strong>{evt.ReservationDate:d MMMM yyyy}</strong> is now complete.</p>
              <table style="width:100%; border-collapse:collapse; margin:16px 0;">
                <tr><td><strong>Order</strong></td><td>{evt.OrderNumber}</td></tr>
                <tr><td><strong>Event Date</strong></td><td>{evt.ReservationDate:d MMMM yyyy}</td></tr>
              </table>
              {depositHtml}
              {notesHtml}
              <p style="margin-top:24px;">
                <a href="{trackingUrl}" style="display:inline-block; background:#c4953a; color:#fff; padding:10px 20px; text-decoration:none; border-radius:4px;">View Order Summary</a>
              </p>
              <p style="margin-top:24px; color:#c4953a;">Thank you for choosing Teacup Boutique — we hope your occasion was truly special.</p>
            </div>
            """;

        await emailService.SendAsync(new EmailMessage
        {
            //To = evt.CustomerEmail,
            To = "luke.birchenough@outlook.com",
            ToName = evt.CustomerName,
            Subject = $"Your booking is complete – {evt.OrderNumber}",
            HtmlBody = html
        });
    }

    public async Task SendEmailChangedAsync(EmailChangedEvent evt)
    {
        string MakeHtml(string heading, string body) => $"""
            <div style="font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 24px; color: #3d1f0d;">
              <h1 style="color:#c4953a;">{heading}</h1>
              <p>Hi {evt.FullName},</p>
              <p>{body}</p>
              <table style="width:100%; border-collapse:collapse; margin:16px 0;">
                <tr><td><strong>Previous email</strong></td><td>{evt.OldEmail}</td></tr>
                <tr><td><strong>New email</strong></td><td>{evt.NewEmail}</td></tr>
              </table>
              <p style="margin-top:16px; padding:16px; background:#fff3cd; border-radius:4px; font-size:14px; color:#856404;">
                If you did not make this change, please contact us immediately.
              </p>
              <p style="margin-top:24px; color:#c4953a;">Teacup Boutique</p>
            </div>
            """;

        await emailService.SendAsync(new EmailMessage
        {
            //To = evt.OldEmail,
            To = "luke.birchenough@outlook.com",
            ToName = evt.FullName,
            Subject = "Your email address has been changed",
            HtmlBody = MakeHtml("Email Address Changed", "Your Teacup Boutique account email address has been changed.")
        });

        await emailService.SendAsync(new EmailMessage
        {
            //To = evt.NewEmail,
            To = "luke.birchenough@outlook.com",
            ToName = evt.FullName,
            Subject = "Welcome to your new email address",
            HtmlBody = MakeHtml("Email Address Updated", "Your Teacup Boutique account is now linked to this email address.")
        });
    }

    public async Task SendEmailVerificationAsync(EmailVerificationRequestedEvent evt)
    {
        var html = $"""
            <div style="font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 24px; color: #3d1f0d;">
              <h1 style="color:#c4953a;">Verify Your Email Address</h1>
              <p>Welcome to Teacup Boutique!</p>
              <p>Please click the button below to verify your email address and activate your account.</p>
              <p style="margin-top:24px;">
                <a href="{evt.VerificationLink}" style="display:inline-block; background:#c4953a; color:#fff; padding:12px 24px; text-decoration:none; border-radius:4px; font-size:16px;">Verify My Email</a>
              </p>
              <p style="margin-top:16px; font-size:13px; color:#888;">This link expires in 1 day. If you did not create an account, you can safely ignore this email.</p>
              <p style="margin-top:24px; color:#c4953a;">Teacup Boutique</p>
            </div>
            """;

        await emailService.SendAsync(new EmailMessage
        {
            //To = evt.Email,
            To = "luke.birchenough@outlook.com",
            ToName = evt.Email,
            Subject = "Verify your Teacup Boutique account",
            HtmlBody = html
        });
    }

    public async Task SendEmailChangeVerificationAsync(EmailChangeVerificationRequestedEvent evt)
    {
        var html = $"""
            <div style="font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 24px; color: #3d1f0d;">
              <h1 style="color:#c4953a;">Confirm Your New Email Address</h1>
              <p>A request was made to change your Teacup Boutique account email to this address.</p>
              <p>Click the button below to confirm and complete the change.</p>
              <p style="margin-top:24px;">
                <a href="{evt.VerificationLink}" style="display:inline-block; background:#c4953a; color:#fff; padding:12px 24px; text-decoration:none; border-radius:4px; font-size:16px;">Confirm New Email</a>
              </p>
              <p style="margin-top:16px; padding:16px; background:#fff3cd; border-radius:4px; font-size:14px; color:#856404;">
                If you did not request this change, you can safely ignore this email. Your current email address will remain unchanged.
              </p>
              <p style="margin-top:24px; color:#c4953a;">Teacup Boutique</p>
            </div>
            """;

        await emailService.SendAsync(new EmailMessage
        {
            //To = evt.NewEmail,
            To = "luke.birchenough@outlook.com",
            ToName = evt.NewEmail,
            Subject = "Confirm your new email address — Teacup Boutique",
            HtmlBody = html
        });
    }
}
