using FinGuard.Application.Commons.Interfaces;
using FinGuard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<
            FinGuard.Infrastructure.MultiTenancy.ITenantProvider,
            FinGuard.Infrastructure.MultiTenancy.FakeTenantProvider>();

        var connectionString = configuration.GetConnectionString("DefaultConnectionString")
                               ?? throw new ArgumentNullException("Connection string not found");

        services.AddDbContext<FinGuardDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        // IFinGuardDbContext injection
        services.AddScoped<IFinGuardDbContext>(provider =>
            provider.GetRequiredService<FinGuardDbContext>());

        return services;
    }
}
