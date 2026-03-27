using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record class RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    [Required] string TurnstileToken
);
