namespace auth.Models;

public record EmailVerificationRequestedEvent(string UserId, string Email, string Token);
public record EmailChangeVerificationRequestedEvent(string UserId, string NewEmail, string Token);
