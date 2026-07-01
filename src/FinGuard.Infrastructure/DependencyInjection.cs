using FinGuard.Application.Commons.Interfaces;
using FinGuard.Infrastructure.Auth;
using FinGuard.Infrastructure.MultiTenancy;
using FinGuard.Infrastructure.Persistence;
using FinGuard.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FinGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

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
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        var jwtSecret = configuration["JwtSettings:Secret"]
                        ?? throw new InvalidOperationException("JWT Secret missing!");
        var key = Encoding.UTF8.GetBytes(jwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Search incoming browser cookies for the authentication token
                    if (context.Request.Cookies.TryGetValue("X-Access-Token", out var token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });


        return services;
    }
}
