namespace auth.Models;

public record EmailVerificationRequestedEvent(string UserId, string Email, string VerificationLink);
public record EmailChangeVerificationRequestedEvent(string UserId, string NewEmail, string VerificationLink);
