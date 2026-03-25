namespace auth.Models;

public record EmailChangedEvent(string UserId, string FullName, string OldEmail, string NewEmail);
