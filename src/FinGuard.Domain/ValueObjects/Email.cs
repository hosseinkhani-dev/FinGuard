using FinGuard.Domain.Exceptions;

namespace FinGuard.Domain.ValueObjects;

public record Email
{
    public string EmailAddress { get; }

    public Email(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress) ||
            !emailAddress.Contains("@"))
            throw new DomainException("Invalid email address format.");

        EmailAddress = emailAddress;
    }
}
