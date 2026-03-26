using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record ResetPasswordRequest(
    [Required][EmailAddress] string Email,
    [Required] string Token,
    [Required][MinLength(6)] string NewPassword
);
