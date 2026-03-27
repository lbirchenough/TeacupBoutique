using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record ForgotPasswordRequest(
    [Required][EmailAddress] string Email,
    [Required] string TurnstileToken
);
