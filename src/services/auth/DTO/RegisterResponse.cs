namespace auth.DTO;

public record RegisterResponse(bool RequiresVerification, string? AccessToken = null);
