namespace auth.Models;

public record PasswordResetRequestedEvent(string Email, string Token);
