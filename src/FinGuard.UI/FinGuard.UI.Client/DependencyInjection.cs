using FinGuard.UI.Client.Features.Tenants.CreateTenants.Services;
using FinGuard.UI.Client.Features.Tenants.Services;

namespace FinGuard.UI.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddClientFeatureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string backendUrl = configuration["ApiSettings:BackendUrl"]
            ?? "https://localhost:2026/";

        services.AddHttpClient<ITenantService, TenantService>(client =>
        {
            client.BaseAddress = new Uri(backendUrl);
        });

        return services;
    }
}
