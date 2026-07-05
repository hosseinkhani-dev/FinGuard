using FinGuard.UI.Common;
using FinGuard.UI.Models.Tenants;

namespace FinGuard.UI.Services.Tenants;

public interface ITenantService
{
    Task<ServiceResult<Guid>> CreateTenantAsync(CreateTenantInputModel input);
}
