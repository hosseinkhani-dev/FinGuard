namespace FinGuard.Application.Features.TransactionFiles.DTOs;

public class ExcelTransactionImportRowDto
{
    public string EmployeeName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public string CardNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Merchant { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string? Description { get; set; }
}
