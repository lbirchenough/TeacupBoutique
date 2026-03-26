using System.ComponentModel.DataAnnotations;

namespace auth.DTO;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required][MinLength(6)] string NewPassword
);
