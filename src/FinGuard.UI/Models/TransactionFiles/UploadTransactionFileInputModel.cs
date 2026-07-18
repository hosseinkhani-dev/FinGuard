using System.ComponentModel.DataAnnotations;

namespace FinGuard.UI.Models.TransactionFiles;

public class UploadTransactionFileInputModel
{
    [Required(ErrorMessage = "Please select a file to upload.")]
    public IFormFile File { get; set; }
    public string? OriginalFileName { get; set; }
    public long FileSize { get; set; }
}
