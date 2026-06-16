
namespace FinGuard.Infrastructure.MultiTenancy;

public class FakeTenantProvider : ITenantProvider
{
    private static readonly Guid DefaultDevTenantId = Guid.Parse("101780fd-6639-4e77-b57a-34ae9f409659");

    public Guid GetTenantId()
    {
        return DefaultDevTenantId;
    }
}
