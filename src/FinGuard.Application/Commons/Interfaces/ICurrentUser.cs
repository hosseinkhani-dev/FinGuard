namespace FinGuard.Application.Commons.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string UserName { get; }
    bool IsAuthenticated { get; }
}
