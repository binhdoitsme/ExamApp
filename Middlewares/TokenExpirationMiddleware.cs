using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ExamApp.Middlewares;

public class TokenExpirationMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly string loginPath = configuration["Authentication:LoginPath"] ?? "/Account/Login";

	public async Task InvokeAsync(HttpContext context)
    {
        // Skip if not authenticated
        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            await next(context);
            return;
        }

        // Skip login, API, and static files
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path?.StartsWith("/account/login") == true ||
            path?.StartsWith("/api/") == true ||
            path?.StartsWith("/_framework") == true ||
            context.Request.Path.Value!.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.Value!.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Check token expiration
        if (IsTokenExpired(context.User))
        {
            // Clear auth cookie/session
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Redirect to login with return URL
            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"{loginPath}?returnUrl={returnUrl}");
            return;
        }

        await next(context);
    }

    private static bool IsTokenExpired(ClaimsPrincipal user)
    {
        // Method 1: Check exp claim directly (most reliable)
        var expClaim = user.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        if (expClaim != null && long.TryParse(expClaim.Value, out var expUnix))
        {
            var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            return expDate < DateTime.UtcNow.AddMinutes(-5); // 5min buffer
        }

        // Method 2: Check .AspNetCore.Identity.Application cookie expiration
        var authResult = user.Identities.FirstOrDefault();
        if (authResult?.AuthenticationType == "Cookies")
        {
            var expiresClaim = authResult.Claims.FirstOrDefault(c => c.Type == "exp");
            if (expiresClaim != null && long.TryParse(expiresClaim.Value, out var cookieExp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(cookieExp).UtcDateTime < DateTime.UtcNow;
            }
        }

        return false;
    }
}

// Extension method for easy registration
public static class TokenExpirationMiddlewareExtensions
{
	public static IApplicationBuilder UseTokenExpirationCheck(
		this IApplicationBuilder builder) => builder.UseMiddleware<TokenExpirationMiddleware>();
}
