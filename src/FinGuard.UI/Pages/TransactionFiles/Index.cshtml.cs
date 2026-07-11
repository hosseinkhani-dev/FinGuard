using FinGuard.UI.Common;
using FinGuard.UI.Services.TransactionFiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinGuard.UI.Pages.TransactionFiles;

public class IndexModel : PageModel
{
    private readonly ITransactionFileService _transactionFileService;

    public IndexModel(ITransactionFileService transactionFileService)
    {
        _transactionFileService = transactionFileService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostDownloadAsync()
    {
        var result = await _transactionFileService.DownloadTemplateAsync();

        if (!result.IsSuccess)
        {
            ModelState.AddServiceErrors(result.Errors, string.Empty);
            return Page();
        }

        return File(result.Value.FileBytes, result.Value.ContentType, result.Value.FileName);
    }
}
