using FinGuard.Domain.Exceptions;

namespace FinGuard.Domain.Entities;

public class RefreshToken
{
    public string Token { get; private set; } = null!;
    public DateTime ExpiryTime { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() { }

    public RefreshToken(string token, DateTime currentTime, DateTime expiryTime)
    {
        if(expiryTime <= currentTime)
        {
            throw new DomainException("Expiry time must be after the creation time.");
        }

        Token = token;
        ExpiryTime = expiryTime;
        CreatedAt = currentTime;
        IsRevoked = false;
    }

    public bool IsExpired(DateTime currentTime) => currentTime >= ExpiryTime;

    public bool IsActive(DateTime currentTime) => !IsRevoked && !IsExpired(currentTime);

    public void Revoke() => IsRevoked = true;
}
