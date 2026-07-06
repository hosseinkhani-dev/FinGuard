using FinGuard.UI.Common;
using FinGuard.UI.Models.Auth;
using FinGuard.UI.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinGuard.UI.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.LoginAsync(Input);

        if (!result.IsSuccess)
        {
            ModelState.AddServiceErrors(result.Errors, prefix: nameof(Input));
            return Page();
        }

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }

        return RedirectToPage("/Index");
    }
}
