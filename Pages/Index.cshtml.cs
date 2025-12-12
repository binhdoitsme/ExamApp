using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace ExamApp.Pages;

public class IndexDto
{
    public string? ErrorMessage { get; set; }
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Error { get; set; }

    public IndexDto Model { get; set; } = new();

    public async Task OnGetAsync()
    {
        Model.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;

        if (Model.IsAuthenticated)
        {
            Model.UserName = User.Identity?.Name ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            Model.UserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }
        else if (!string.IsNullOrEmpty(Error))
        {
            // ✅ Custom error from login failure
            Model.ErrorMessage = Error switch
            {
                "AccessDenied" => "Login cancelled or access denied.",
                "InvalidCredentials" => "Invalid login credentials.",
                "NotWhitelisted" => "User is not allowed to use ExamApp!",
                _ => "Login failed. Please try again."
            };
        }
    }

    public IActionResult OnPostLogout() => SignOut(
        new AuthenticationProperties
        {
            RedirectUri = "/"
        },
        "Google", "Cookies");
}
