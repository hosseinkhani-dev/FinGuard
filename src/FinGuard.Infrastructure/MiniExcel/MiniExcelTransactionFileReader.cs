using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.TransactionFiles.DTOs;
using MiniExcelLibs;
using System.Runtime.CompilerServices;

namespace FinGuard.Infrastructure.MiniExcel;

public class MiniExcelTransactionFileReader : ITransactionFileReader
{
    public async Task<IReadOnlyList<ExcelTransactionImportRowDto>> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rows = await stream.QueryAsync<ExcelTransactionImportRowDto>();

        return rows.ToList();
    }
}
