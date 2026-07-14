using FinGuard.Domain.Common;
using FinGuard.Domain.Exceptions;

namespace FinGuard.Domain.Entities;

public class Transaction : ITenant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid TransactionFileId { get; private set; }
    public string EmployeeName { get; private set; } = null!;
    public string Department { get; private set; } = null!;
    public DateTime TransactionDate { get; private set; }
    public string CardNumber { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Merchant { get; private set; } = null!;
    public string Currency { get; private set; } = null!;
    public string? Description { get; set; }

    // Navigation Property
    public TransactionFile TransactionFile { get; private set; } = null!;

    public Transaction(
        Guid transactionFileId,
        string employeeName,
        string department,
        DateTime transactionDate,
        string cardNumber,
        decimal amount,
        string merchant,
        string currency,
        string? description)
    {
        Id = Guid.NewGuid();

        TransactionFileId = transactionFileId;

        SetEmployeeName(employeeName);
        SetDepartment(department);
        SetTransactionDate(transactionDate);
        SetCardNumber(cardNumber);
        SetAmount(amount);
        SetMerchant(merchant);
        SetCurrency(currency);
        SetDescription(description);
    }

    public void AssignTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("Tenant ID cannot be empty.");

        if (TenantId != Guid.Empty)
            throw new DomainException("Cannot reassign a user to a different tenant.");

        TenantId = tenantId;
    }

    private void SetEmployeeName(string employeeName)
    {
        if (string.IsNullOrWhiteSpace(employeeName))
            throw new DomainException("Employee name is required.");

        if (employeeName.Length > 50)
            throw new DomainException("Employee name cannot exceed 100 characters.");

        EmployeeName = employeeName.Trim();
    }

    private void SetDepartment(string department)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new DomainException("Department is required.");

        if (department.Length > 50)
            throw new DomainException("Department cannot exceed 100 characters.");

        Department = department.Trim();
    }

    private void SetTransactionDate(DateTime transactionDate)
    {
        TransactionDate = transactionDate;
    }

    private void SetCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new DomainException("Card number is required.");

        if (cardNumber.Length > 30)
            throw new DomainException("Card number cannot exceed 30 characters.");

        CardNumber = cardNumber.Trim();
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        Amount = amount;
    }

    private void SetMerchant(string merchant)
    {
        if (string.IsNullOrWhiteSpace(merchant))
            throw new DomainException("Merchant is required.");

        if (merchant.Length > 100)
            throw new DomainException("Merchant cannot exceed 100 characters.");

        Merchant = merchant.Trim();
    }

    private void SetCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        if (currency.Length > 10)
            throw new DomainException("Currency cannot exceed 10 characters.");

        Currency = currency.Trim().ToUpperInvariant();
    }

    private void SetDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
        }
        else
        {
            if (description!.Length > 500)
                throw new DomainException("Description cannot exceed 500 characters.");

            Description = description.Trim();
        }
    }
}
