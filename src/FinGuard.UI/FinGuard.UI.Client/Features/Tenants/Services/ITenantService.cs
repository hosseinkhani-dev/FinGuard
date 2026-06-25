using FinGuard.UI.Client.Components.Shared;
using FinGuard.UI.Client.Features.Tenants.Models;

namespace FinGuard.UI.Client.Features.Tenants.Services;

public interface ITenantService
{
    Task<ModelResult<Guid>> CreateTenantAsync(CreateTenantModel model);
}
