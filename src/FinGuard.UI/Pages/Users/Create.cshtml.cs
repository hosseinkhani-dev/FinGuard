using FinGuard.UI.Common;
using FinGuard.UI.Models.Users;
using FinGuard.UI.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinGuard.UI.Pages.Users;

public class CreateModel : PageModel
{
    private readonly IUserService _userService;

    public CreateModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public CreateUserInputModel InputModel { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var result = await _userService.CreateUserAsync(InputModel);

        if (!result.IsSuccess)
        {
            ModelState.AddServiceErrors(result.Errors, nameof(InputModel));
            return Page();
        }

        SuccessMessage = $"User '{InputModel.UserName}' created successfully.";
        return RedirectToPage("/Users/Create", new { id = result.Value });
    }
}
