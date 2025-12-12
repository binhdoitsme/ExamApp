using ExamApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExamApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth");
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromServices] WhitelistService whitelistService, CancellationToken cancellationToken = default)
    {
        var result = await HttpContext.AuthenticateAsync();

        if (!result.Succeeded)
        {
            return Redirect("/?error=InvalidCredentials");
        }

        // Extract email from authentication result
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            return Redirect("/?error=InvalidCredentials");
        }

        // Check if the user is whitelisted
        if (!await whitelistService.IsWhitelistedUser(email, cancellationToken))
        {
            // Logout/unauthenticate if not whitelisted
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/?error=NotWhitelisted");
        }

        return LocalRedirect("/");
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect("/");
    }
}
