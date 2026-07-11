using FinGuard.UI.Infrastructure.Api;
using FinGuard.UI.Services.Auth;
using FinGuard.UI.Services.Tenants;
using FinGuard.UI.Services.TransactionFiles;
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
        services.AddHttpClient(
            "AuthRefreshClient",
            client =>
            {
                client.BaseAddress = new Uri(backendUrl);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            new HttpClientHandler
            {
                        UseCookies = false
            });

        services.AddTransient<BearerTokenHandler>();
        services.AddTransient<TokenRefreshHandler>();
        services.AddScoped<TokenSessionService>();

        // Dependency Injections
        services.AddHttpClient<ITenantService, TenantService>(configureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<IAuthService, AuthService>(configureClient);

        services.AddHttpClient<IUserService, UserService>(configureClient)
            .AddHttpMessageHandler<TokenRefreshHandler>()
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<ITransactionFileService, TransactionFileService>(configureClient)
            .AddHttpMessageHandler<TokenRefreshHandler>()
            .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }
}
