using FinGuard.Domain.Common;
using FinGuard.Domain.Enums;
using FinGuard.Domain.Exceptions;
using FinGuard.Domain.ValueObjects;

namespace FinGuard.Domain.Entities;

public class User : ITenant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string UserName { get; private set; }
    public string PasswordHash { get; private set; }
    public Email? Email { get; private set; }
    public UserRole Role { get; private set; }

    private User() { }

    public User(
        //Guid tenantId,
        string userName,
        string passwordHash,
        Email? email)
    {
        //if (tenantId == Guid.Empty)
        //    throw new DomainException("Tenant Id cannot be null.");

        Id = Guid.NewGuid();
        SetUserName(userName);
        //TenantId = tenantId;
        PasswordHash = passwordHash;
        Email = email;
        Role = UserRole.Auditor;
    }

    public void UpdateUserName(string userName)
    {
        SetUserName(userName);
    }

    private void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("UserName cannot be empty.");

        if (userName.Length > 50)
            throw new DomainException("UserName cannot be more than 50 character.");

        UserName = userName;
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
