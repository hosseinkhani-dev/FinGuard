using FinGuard.Domain.Exceptions;

namespace FinGuard.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency cannot be empty.");

        if (currency.Trim().Length != 3)
            throw new DomainException("Currency must be a valid 3-letter ISO code (e.g., USD, EUR).");

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }
}
