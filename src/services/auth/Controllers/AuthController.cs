using auth.DTO;
using auth.Models;
using auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(TokenService tokenService, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
        {
            var user = new ApplicationUser
            {
                UserName = req.Email,
                Email = req.Email
            };

            var result = await userManager.CreateAsync(user, req.Password);

            if (!result.Succeeded)
                return BadRequest(string.Join(" ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, "User");
            await SetRefreshTokenCookie(user);

            var token = await tokenService.CreateAccessToken(user);
            return Ok(new AuthResponse(token));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null)
                return Unauthorized("Invalid credentials");

            
            var ok = await userManager.CheckPasswordAsync(user, req.Password);
            //look into signup manager more after
            //var ok = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false); 
            if (!ok)
                return Unauthorized("Invalid credentials");

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

        // private async Task<bool> EmailExists(string email)
        // {
        //     //Adding ! to x.Email to assert with the null suppression operator that this value won't be null at runtime
        //     return await context.Users.AnyAsync(x => x.Email!.ToLower() == email.ToLower());
        // }

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
                // SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7) //will get removed from browser after 7 days
            };

            //Append cookie RefreshToken to the response 
            Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

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
