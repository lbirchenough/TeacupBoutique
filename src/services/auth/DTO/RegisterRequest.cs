using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record class RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password
);
