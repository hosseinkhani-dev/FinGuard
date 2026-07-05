using FinGuard.UI.Common;
using FinGuard.UI.Models.Tenants;
using FinGuard.UI.Services.Tenants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinGuard.UI.Pages.Tenants;

public class CreateModel : PageModel
{
    private readonly ITenantService _tenantService;

    public CreateModel(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [BindProperty]
    public CreateTenantInputModel Input { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _tenantService.CreateTenantAsync(Input);

        if (!result.IsSuccess)
        {
            ModelState.AddServiceErrors(result.Errors, nameof(Input));
            return Page();
        }

        SuccessMessage = $"Tenant '{Input.Name}' with Username {Input.UserName} created successfully.";

        return RedirectToPage("/Tenants/Create", new { id = result.Value });
    }
}
