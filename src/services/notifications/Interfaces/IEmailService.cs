using notifications.Models;

namespace notifications.Interfaces;

public interface IEmailService
{
    Task SendAsync(EmailMessage message);
}
