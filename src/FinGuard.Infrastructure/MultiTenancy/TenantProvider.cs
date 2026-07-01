using FinGuard.Application.Commons.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FinGuard.Infrastructure.MultiTenancy;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if(user?.Identity?.IsAuthenticated != true)
        {
            return Guid.Empty;
        }

        var tenantClaim = user?.FindFirst("tenant_id")?.Value;

        if (Guid.TryParse(tenantClaim, out var tenantId))
        {
            return tenantId;
        }

        return Guid.Empty;
    }
}
