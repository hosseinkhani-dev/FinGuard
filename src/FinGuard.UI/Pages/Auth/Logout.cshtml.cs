using FinGuard.UI.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinGuard.UI.Pages.Auth;

public class LogoutModel : PageModel
{
    private readonly IAuthService _authService;

    public LogoutModel(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _authService.LogoutAsync();

        // Clear transient data to prevent dirty state leakage
        HttpContext.Response.Cookies.Delete("FinGuard.UI.Session");

        return RedirectToPage("/Auth/Login");
    }
}
