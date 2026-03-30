using Microsoft.Extensions.Logging;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Services;

public class MailgunEmailService(HttpClient httpClient, IConfiguration config, ILogger<MailgunEmailService> logger) : IEmailService
{
    private readonly string _domain = config["Mailgun:Domain"]!;
    private readonly string _fromAddress = config["Mailgun:FromAddress"]!;
    private readonly string _fromName = config["Mailgun:FromName"] ?? "Teacup Boutique";

    public async Task SendAsync(EmailMessage message)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["from"] = $"{_fromName} <{_fromAddress}>",
            ["to"] = $"{message.ToName} <{message.To}>",
            ["subject"] = message.Subject,
            ["html"] = message.HtmlBody,
        });

        var response = await httpClient.PostAsync($"v3/{_domain}/messages", form);
        response.EnsureSuccessStatusCode();
        logger.LogInformation("Email sent to {To} — {Subject}", message.To, message.Subject);
    }
}
