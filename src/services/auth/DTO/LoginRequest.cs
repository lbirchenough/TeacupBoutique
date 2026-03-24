using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record class LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);
