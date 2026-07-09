using FinGuard.UI.Common;
using FinGuard.UI.Models.Users;
using FinGuard.UI.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FinGuard.UI.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchUserName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchEmail { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? SearchIsActive { get; set; }

        public List<GetAllUsersModel> Users { get; set; } = new();

        public List<string> ErrorMessages { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _userService.GetAllUsersAsync(SearchUserName, SearchEmail, SearchIsActive);

            if (!result.IsSuccess)
            {
                // Assign error messages to be displayed in a validation summary or alert box
                ModelState.AddServiceErrors(result.Errors);
                return Page();
            }

            Users = result.Value ?? new List<GetAllUsersModel>();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id, string currentStatus)
        {
            ServiceResult<bool> result;

            if (currentStatus == "Active")
            {
                result = await _userService.DisableUserAsync(id);
            }
            else
            {
                result = await _userService.ActivateUserAsync(id);
            }

            if (!result.IsSuccess)
            {
                // Transfer API errors to ModelState to display in the validation summary
                ModelState.AddServiceErrors(result.Errors);

                // Reload the user list so the page renders properly with the error
                var usersResult = await _userService.GetAllUsersAsync(SearchUserName, SearchEmail, SearchIsActive);
                Users = usersResult.Value ?? new List<GetAllUsersModel>();
                return Page();
            }

            // Refresh the page using the current search/filter parameters
            return RedirectToPage("./Index", new
            {
                SearchUserName,
                SearchEmail,
                SearchIsActive
            });
        }
    }
}
