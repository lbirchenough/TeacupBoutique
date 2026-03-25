using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record UpdateProfileRequest(
    string? FullName,
    [Required][EmailAddress] string Email,
    string? PhoneNumber
);
