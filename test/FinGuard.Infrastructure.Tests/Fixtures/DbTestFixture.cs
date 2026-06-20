using FinGuard.Infrastructure.MultiTenancy;
using FinGuard.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Respawn;
using Testcontainers.MsSql;

namespace FinGuard.Infrastructure.Tests.Fixtures;

public class DbTestFixture : IAsyncLifetime
{
    public MsSqlContainer MsSqlContainer { get; } = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private Respawner _respawn = null!;

    public async ValueTask InitializeAsync()
    {
        await MsSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<FinGuardDbContext>()
            .UseSqlServer(MsSqlContainer.GetConnectionString())
            .Options;

        var dummyTenantProvider = Substitute.For<ITenantProvider>();
        using (var context = new FinGuardDbContext(options, dummyTenantProvider))
        {
            await context.Database.EnsureCreatedAsync();
        }

        using var connection = new SqlConnection(MsSqlContainer.GetConnectionString());
        await connection.OpenAsync();

        _respawn = await Respawner.CreateAsync(
            connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
            });

    }

    public async Task ResetDatabaseAsync()
    {
        using var connection = new SqlConnection(MsSqlContainer.GetConnectionString());
        await connection.OpenAsync();

        await _respawn.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        await MsSqlContainer.DisposeAsync();
    }
}
