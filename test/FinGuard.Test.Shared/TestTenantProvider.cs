using FinGuard.Infrastructure.MultiTenancy;

namespace FinGuard.Test.Shared;

public class TestTenantProvider : ITenantProvider
{
    public Guid CurrentTenantId { get; set; } = Guid.NewGuid();
    public Guid GetTenantId() => CurrentTenantId;
}
