using FinGuard.UI.Common;
using FinGuard.UI.Models.TransactionFiles;
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

    [BindProperty]
    public UploadTransactionFileInputModel Input { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

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

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (Input.File != null)
        {
            // Explicitly set the properties so binder validation passes
            Input.OriginalFileName = Input.File.FileName;
            Input.FileSize = Input.File.Length;
        }

        // Clear previous model state for these two fields and re-evaluate
        ModelState.Remove(nameof(Input.OriginalFileName));
        ModelState.Remove(nameof(Input.FileSize));

        if (!TryValidateModel(Input))
        {
            return Page();
        }

        var result = await _transactionFileService.UploadTransactionFileAsync(Input);

        if (!result.IsSuccess)
        {
            ModelState.AddServiceErrors(result.Errors, string.Empty);
            return Page();
        }

        SuccessMessage = $"File uploaded successfully and queued for processing. File ID: {result.Value}";

        return RedirectToPage();
    }
}
