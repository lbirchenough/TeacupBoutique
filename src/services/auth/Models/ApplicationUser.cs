using System;
using Microsoft.AspNetCore.Identity;

namespace auth.Models;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? FullName { get; set; }
}
