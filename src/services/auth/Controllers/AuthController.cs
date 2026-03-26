using System.Text.Json;
using auth.DTO;
using auth.Interfaces;
using auth.Models;
using auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(TokenService tokenService, UserManager<ApplicationUser> userManager, IMessagePublisher publisher, IConfiguration configuration) : ControllerBase
    {
        private readonly string clientUrl = configuration["ClientUrl"] ?? "https://localhost:5173";

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest req)
        {
            var user = new ApplicationUser
            {
                UserName = req.Email,
                Email = req.Email
            };

            var result = await userManager.CreateAsync(user, req.Password);

            if (!result.Succeeded)
                return BadRequest(string.Join(" ", result.Errors.Select(e => e.Description)));

            var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = $"{clientUrl}/verify-email?token={Uri.EscapeDataString(confirmToken)}&email={Uri.EscapeDataString(user.Email!)}";
            await publisher.PublishAsync("auth.EmailVerificationRequested",
                JsonSerializer.Serialize(new EmailVerificationRequestedEvent(user.Id, user.Email!, link)));

            return Ok(new RegisterResponse(RequiresVerification: true));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null)
                return Unauthorized("Invalid credentials");

            var ok = await userManager.CheckPasswordAsync(user, req.Password);
            if (!ok)
                return Unauthorized("Invalid credentials");

            if (!user.EmailConfirmed)
                return Unauthorized("Please verify your email before logging in.");

            await SetRefreshTokenCookie(user);

            var token = await tokenService.CreateAccessToken(user);
            return Ok(new AuthResponse(token));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken()
        {
            //Basically have a timer in SPA that hits this end point every 5 minutes
            //If user is authenticated with valid refresh token, itll continually make a new refresh token with fresh 7 day expiry every 5 minutes
            //If user then doesn't use app for 7 days it will be invalidated and user would need to login again to reauthenticate

            var refreshToken = Request.Cookies["RefreshToken"];
            if (refreshToken == null) return NoContent();

            var user = await userManager.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.RefreshTokenExpiry > DateTime.UtcNow);

            if (user == null) return Unauthorized();

            await SetRefreshTokenCookie(user);

            var token = await tokenService.CreateAccessToken(user);
            return Ok(new AuthResponse(token));
        }

        [HttpGet("verify-email")]
        public async Task<ActionResult<AuthResponse>> VerifyEmail([FromQuery] VerifyEmailRequest req)
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null) return BadRequest("Invalid link.");

            var result = await userManager.ConfirmEmailAsync(user, req.Token);
            if (!result.Succeeded) return BadRequest("Invalid or expired link.");

            await userManager.AddToRoleAsync(user, "User");
            await SetRefreshTokenCookie(user);
            return Ok(new AuthResponse(await tokenService.CreateAccessToken(user)));
        }

        [HttpGet("verify-email-change")]
        public async Task<ActionResult<AuthResponse>> VerifyEmailChange([FromQuery] VerifyEmailChangeRequest req)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.PendingEmail == req.NewEmail);
            if (user is null) return BadRequest("Invalid link.");

            var oldEmail = user.Email!;
            var result = await userManager.ChangeEmailAsync(user, req.NewEmail, req.Token);
            if (!result.Succeeded) return BadRequest("Invalid or expired link.");

            await userManager.SetUserNameAsync(user, req.NewEmail);
            await userManager.UpdateSecurityStampAsync(user);
            user.PendingEmail = null;
            await userManager.UpdateAsync(user);

            var evt = new EmailChangedEvent(user.Id, user.FullName ?? "", oldEmail, req.NewEmail);
            await publisher.PublishAsync("auth.EmailChanged", JsonSerializer.Serialize(evt));

            await SetRefreshTokenCookie(user);
            return Ok(new AuthResponse(await tokenService.CreateAccessToken(user)));
        }

        private async Task SetRefreshTokenCookie(ApplicationUser user)
        {
            //We want a refresh token that lives up to 7 days in users browser but is NOT accessible from our angular app or any javascript
            //We would then get this cookie with token back on every client request and we can verify on login against refresh token stored in db and then issue a short lived token
            //Short lived token is then only stored in angular memory not in local storage
            //This is more secure than just storing token in local storage on client browser.

            //Generate a token and update user in db with RefreshToken and RefreshTokenExpiry
            var refreshToken = tokenService.GeneratateRefreshToken();
            //Both RefreshToken and RefreshTokenExpiry are columns in user table added by IdentityFramework
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            //Create cookie options with HttpOnly and long expiry
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, //HttpOnly cookies are not accessible from client side javascript / apps, not accessible clientside
                Secure = true, //only sent over https so ensure using https in dev
                SameSite = SameSiteMode.Strict, // samesite is for controlling if a cookie gets sent, same site considers scheme + hostname NOT PORT, so I just needed to make sure angular was running on https and the cookies started being sent
                Expires = DateTime.UtcNow.AddDays(7) //will get removed from browser after 7 days
            };

            //Append cookie RefreshToken to the response
            Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is not null && user.EmailConfirmed)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var link = $"{clientUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(req.Email)}";
                await publisher.PublishAsync("auth.PasswordResetRequested",
                    JsonSerializer.Serialize(new PasswordResetRequestedEvent(req.Email, link)));
            }
            return Ok(); // always 200 — don't reveal whether email exists
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null) return BadRequest("Invalid request.");

            var result = await userManager.ResetPasswordAsync(user, req.Token, req.NewPassword);
            if (!result.Succeeded)
                return BadRequest(string.Join(" ", result.Errors.Select(e => e.Description)));

            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ProfileResponse>> GetProfile()
        {
            var userId = User.GetMemberId();
            var user = await userManager.FindByIdAsync(userId!);
            if (user is null) return NotFound();

            return Ok(new ProfileResponse(
                user.FullName ?? "",
                user.Email ?? "",
                user.PhoneNumber ?? ""));
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.GetMemberId();
            var user = await userManager.FindByIdAsync(userId!);
            if (user is null) return NotFound();

            var emailChanged = !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase);

            if (emailChanged)
            {
                var existing = await userManager.FindByEmailAsync(request.Email);
                if (existing is not null && existing.Id != user.Id)
                    return BadRequest("That email address is already in use.");

                var changeToken = await userManager.GenerateChangeEmailTokenAsync(user, request.Email);
                user.PendingEmail = request.Email;
                user.FullName = request.FullName;
                user.PhoneNumber = request.PhoneNumber;
                await userManager.UpdateAsync(user);

                var link = $"{clientUrl}/verify-email-change?token={Uri.EscapeDataString(changeToken)}&newEmail={Uri.EscapeDataString(request.Email)}";
                await publisher.PublishAsync("auth.EmailChangeVerificationRequested",
                    JsonSerializer.Serialize(new EmailChangeVerificationRequestedEvent(user.Id, request.Email, link)));

                return Ok(new { requiresEmailVerification = true });
            }

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(string.Join(" ", result.Errors.Select(e => e.Description)));

            return NoContent();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            var check = await userManager.CheckPasswordAsync(user, req.CurrentPassword);
            if (!check) return BadRequest("Current password is incorrect.");

            var result = await userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
            if (!result.Succeeded)
                return BadRequest(string.Join(" ", result.Errors.Select(e => e.Description)));

            await publisher.PublishAsync("auth.PasswordChanged",
                JsonSerializer.Serialize(new PasswordChangedEvent(user.Email!)));

            return NoContent();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await userManager.Users
                .Where(x => x.Id == User.GetMemberId())
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RefreshToken, _ => null)
                    .SetProperty(x => x.RefreshTokenExpiry, _ => null)
                    );

            Response.Cookies.Delete("RefreshToken");

            return Ok();
        }
    }
}
