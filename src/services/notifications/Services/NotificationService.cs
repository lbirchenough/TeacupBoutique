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
            To = evt.CustomerEmail,
            ToName = evt.CustomerName,
            Subject = $"Booking Cancelled – {evt.OrderNumber}",
            HtmlBody = html
        });
    }
}
