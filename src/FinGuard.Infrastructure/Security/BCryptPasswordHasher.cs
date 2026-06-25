using FinGuard.Application.Commons.Interfaces;

namespace FinGuard.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    private readonly int _workFactor;

    public BCryptPasswordHasher(int workFactor = 11)
    {
        _workFactor = workFactor;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }
}
