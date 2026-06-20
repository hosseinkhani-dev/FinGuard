namespace FinGuard.Infrastructure.MultiTenancy;

public interface ITenantProvider
{
    Guid GetTenantId();
}
