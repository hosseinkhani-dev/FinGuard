using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.TransactionFiles.DTOs;
using FinGuard.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.TransactionFiles.Commands.ImportTransactions;

public class ImportTransactionsCommandHandler :
    IRequestHandler<ImportTransactionsCommand, int>
{
    private const int BatchSize = 500;

    private readonly IFinGuardDbContext _context;
    private readonly IFileStorage _storage;
    private readonly ITransactionFileReader _reader;

    public ImportTransactionsCommandHandler(
        IFinGuardDbContext context,
        IFileStorage storage,
        ITransactionFileReader reader,
        TimeProvider timeProvider)
    {
        _context = context;
        _storage = storage;
        _reader = reader;
    }

    public async Task<int> Handle(
        ImportTransactionsCommand request, CancellationToken cancellationToken)
    {
        var transactionFile = await _context.TransactionFiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(tf => tf.Id == request.TransactionFileId,
            cancellationToken);

        if (transactionFile == null)
            throw new NotFoundException("Transaction file not found.");

        await using var stream =
            await _storage.OpenReadAsync(transactionFile.StoragePath, cancellationToken);

        var rows = await _reader.ReadAsync(stream,cancellationToken);

        var batch = new List<Transaction>();

        var importedRows = 0;

        var rowNumber = 2;

        foreach (var row in rows)
        {
            try
            {
                if (IsEmptyRow(row))
                    continue;

                ValidateRow(row);

                var transaction = new Transaction(
                    transactionFile.Id,
                    row.EmployeeName,
                    row.Department,
                    row.TransactionDate,
                    row.CardNumber,
                    row.Amount,
                    row.Merchant,
                    row.Currency,
                    row.Description);
                transaction.AssignTenantId(transactionFile.TenantId);

                batch.Add(transaction);
                importedRows++;


                if (batch.Count >= BatchSize)
                {
                    await SaveBatchAsync(
                        batch,
                        cancellationToken);

                    batch.Clear();
                }
            }
            catch (ImportValidationException ex)
            {
                throw new ImportValidationException(
                    $"Row {rowNumber}: {ex.Message}");
            }

            rowNumber++;
        }

        if (batch.Count > 0)
        {
            await SaveBatchAsync(
                batch,
                cancellationToken);
        }

        return importedRows;
    }

    private async Task SaveBatchAsync(
        List<Transaction> transactions,
        CancellationToken cancellationToken)
    {
        await _context.Transactions.AddRangeAsync(
            transactions,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    private static void ValidateRow(ExcelTransactionImportRowDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EmployeeName))
            throw new ImportValidationException(
                "Employee name is required.");

        if (string.IsNullOrWhiteSpace(dto.Department))
            throw new ImportValidationException(
                "Department is required.");

        if (dto.Amount <= 0)
            throw new ImportValidationException(
                "Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.Merchant))
            throw new ImportValidationException(
                "Merchant is required.");

        if (string.IsNullOrWhiteSpace(dto.Currency))
            throw new ImportValidationException(
                "Currency is required.");

        if (dto.TransactionDate == default)
            throw new ImportValidationException(
                "Transaction date is required.");

        if(dto.Description?.Length > 500)
            throw new ImportValidationException(
                "Description cannot exceed 500 characters.");
    }

    private static bool IsEmptyRow(ExcelTransactionImportRowDto row)
    {
        return string.IsNullOrWhiteSpace(row.EmployeeName)
            && string.IsNullOrWhiteSpace(row.Department)
            && string.IsNullOrWhiteSpace(row.CardNumber)
            && string.IsNullOrWhiteSpace(row.Merchant)
            && row.Amount == 0
            && row.TransactionDate == default;
    }
}
