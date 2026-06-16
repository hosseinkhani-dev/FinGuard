namespace FinGuard.Domain.Common;

public interface ITenant
{
    Guid TenantId { get; }
}
