namespace FinGuard.Application.Commons.Interfaces;

public interface ITenantProvider
{
    Guid GetTenantId();
}
