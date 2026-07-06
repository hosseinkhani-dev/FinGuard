using FinGuard.UI.Infrastructure.Api;
using FinGuard.UI.Services.Auth;
using FinGuard.UI.Services.Tenants;
using FinGuard.UI.Services.Users;

namespace FinGuard.UI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var backendUrl = configuration["BackendApiUrl"] ?? "https://localhost:2026/";

        Action<HttpClient> configureClient = client =>
        {
            client.BaseAddress = new Uri(backendUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        };

        // Core infrastructure required for Cookie propagation
        services.AddHttpContextAccessor();
        services.AddTransient<CookiePropagationHandler>();
        services.AddTransient<TokenRefreshHandler>();

        // Dependency Injections
        services.AddHttpClient<ITenantService, TenantService>(configureClient)
            .AddHttpMessageHandler<CookiePropagationHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

        services.AddHttpClient<IAuthService, AuthService>(configureClient)
            .AddHttpMessageHandler<CookiePropagationHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

        services.AddHttpClient<IUserService, UserService>(configureClient)
            .AddHttpMessageHandler<CookiePropagationHandler>();

        return services;
    }
}
