using FinGuard.UI.Services.Tenants;

namespace FinGuard.UI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var backendUrl = configuration["BackendApiUrl"] ?? "https://localhost:2026/";

        Action<HttpClient> configureClient = client =>
        {
            client.BaseAddress = new Uri(backendUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        };

        // Dependency Injections
        services.AddHttpClient<ITenantService, TenantService>(configureClient);

        return services;
    }
}
