using FinGuard.UI.Common;
using FinGuard.UI.Models.TransactionFiles;

namespace FinGuard.UI.Services.TransactionFiles;

public interface ITransactionFileService
{
    Task<ServiceResult<FileDownloadResult>> DownloadTemplateAsync();

    Task<ServiceResult<Guid>> UploadTransactionFileAsync(UploadTransactionFileInputModel inputModel);
}
