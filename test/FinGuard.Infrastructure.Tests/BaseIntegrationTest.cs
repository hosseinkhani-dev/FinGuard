
using FinGuard.Infrastructure.Persistence;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.Test.Shared;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Infrastructure.Tests;

[Collection("DatabaseCollection")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly DbTestFixture _fixture;
    protected  TestTenantProvider TenantProvider;

    protected BaseIntegrationTest(
        DbTestFixture fixture)
    {
        _fixture = fixture;
        TenantProvider = new TestTenantProvider();
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    protected FinGuardDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FinGuardDbContext>()
            .UseSqlServer(_fixture.MsSqlContainer.GetConnectionString())
            .Options;

        return new FinGuardDbContext(options, TenantProvider);
    }
}
