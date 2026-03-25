using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record VerifyEmailRequest([Required][EmailAddress] string Email, [Required] string Token);
