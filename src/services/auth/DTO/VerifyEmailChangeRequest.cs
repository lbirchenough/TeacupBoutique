using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record VerifyEmailChangeRequest([Required][EmailAddress] string NewEmail, [Required] string Token);
