using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinGuard.UI.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var isAuth = User.Identity?.IsAuthenticated;
            var name = User.Identity?.Name;
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}");

            return Content(
                $"Authenticated: {isAuth}\nName: {name}\nClaims:\n{string.Join("\n", claims)}");
        }
    }

}
