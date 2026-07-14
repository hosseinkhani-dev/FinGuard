using FinGuard.Application.Features.TransactionFiles.DTOs;

namespace FinGuard.Application.Commons.Interfaces;

public interface ITransactionFileReader
{
    Task<IReadOnlyList<ExcelTransactionImportRowDto>> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken);
}
