using FinGuard.UI.Common;
using FinGuard.UI.Infrastructure.Api;
using FinGuard.UI.Models.TransactionFiles;

namespace FinGuard.UI.Services.TransactionFiles;

public class TransactionFileService : ITransactionFileService
{
    private readonly HttpClient _httpClient;

    public TransactionFileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceResult<FileDownloadResult>> DownloadTemplateAsync()
    {
        var response = await _httpClient.GetAsync("api/transaction-files/transaction-template");

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ApiErrorHandler.ParseErrorsAsync(response);
            return ServiceResult<FileDownloadResult>.Failure(errors);
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();

        var contentType = response.Content.Headers.ContentType?.MediaType
                          ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                       ?? response.Content.Headers.ContentDisposition?.FileName
                       ?? "Transaction-Template.xlsx";

        var result = new FileDownloadResult
        {
            FileBytes = bytes,
            ContentType = contentType,
            FileName = fileName
        };

        return ServiceResult<FileDownloadResult>.Success(result);
    }
}
