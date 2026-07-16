using FinGuard.Application.Features.TransactionFiles.DTOs;

namespace FinGuard.IntegrationTests.Builders.DTOs;

public class ExcelTransactionImportRowBuilder
{
    private string _employeeName = "John Doe";
    private string _department = "Engineering";
    private DateTime _transactionDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
    private string _cardNumber = "1234-5678";
    private decimal _amount = 150.50m;
    private string _merchant = "AWS Cloud Services";
    private string _currency = "USD";
    private string _description = "Hosting services";

    public ExcelTransactionImportRowBuilder WithEmployeeName(string employeeName)
    {
        _employeeName = employeeName;
        return this;
    }

    public ExcelTransactionImportRowBuilder WithDepartment(string department)
    {
        _department = department;
        return this;
    }

    public ExcelTransactionImportRowBuilder WithTransactionDate(DateTime transactionDate)
    {
        _transactionDate = transactionDate;
        return this;
    }

    public ExcelTransactionImportRowBuilder WithCardNumber(string cardNumber)
    {
        _cardNumber = cardNumber;
        return this;
    }

    public ExcelTransactionImportRowBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public ExcelTransactionImportRowBuilder WithMerchant(string merchant)
    {
        _merchant = merchant;
        return this;
    }

    public ExcelTransactionImportRowBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public ExcelTransactionImportRowBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ExcelTransactionImportRowDto Build()
    {
        return new ExcelTransactionImportRowDto
        {
            EmployeeName = _employeeName,
            Department = _department,
            TransactionDate = _transactionDate,
            CardNumber = _cardNumber,
            Amount = _amount,
            Merchant = _merchant,
            Currency = _currency,
            Description = _description
        };
    }
}
