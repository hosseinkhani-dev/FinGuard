using FinGuard.UI.Common;
using FinGuard.UI.Infrastructure.Api;
using FinGuard.UI.Models.TransactionFiles;
using System.Net.Http.Headers;

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

    public async Task<ServiceResult<Guid>> UploadTransactionFileAsync(UploadTransactionFileInputModel inputModel)
    {
        if (inputModel.File == null || inputModel.FileSize == 0)
        {
            return ServiceResult<Guid>.Failure("No file provided for upload.");
        }

        using var content = new MultipartFormDataContent();

        // Open the file stream directly from the IFormFile structure
        await using var fileStream = inputModel.File.OpenReadStream();
        var streamContent = new StreamContent(fileStream);

        streamContent.Headers.ContentType = new MediaTypeHeaderValue(inputModel.File.ContentType);

        content.Add(streamContent, "file", inputModel.OriginalFileName);

        var response = await _httpClient.PostAsync("api/transaction-files", content);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ApiErrorHandler.ParseErrorsAsync(response);
            return ServiceResult<Guid>.Failure(errors);
        }

        var id = await response.Content.ReadFromJsonAsync<Guid>();
        return ServiceResult<Guid>.Success(id);
    }
}
