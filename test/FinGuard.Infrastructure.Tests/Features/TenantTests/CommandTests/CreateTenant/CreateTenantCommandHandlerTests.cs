using FinGuard.Application.Features.Tenants.Commands.CreateTenant;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FinGuard.IntegrationTests.Features.TenantTests.CommandTests.CreateTenant;

public class CreateTenantCommandHandlerTests : BaseIntegrationTest
{
    private readonly TimeProvider _mockTimeProvider;
    private readonly DateTimeOffset _expectedTime;

    public CreateTenantCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _expectedTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _mockTimeProvider = Substitute.For<TimeProvider>();
        _mockTimeProvider.GetUtcNow().Returns(_expectedTime);
    }

    [Fact]
    public async Task Handle_ShouldRegisterTenantAndUser_WhenDataIsValid()
    {
        // Arrange
        using var context = CreateDbContext();

        var handler = new CreateTenantCommandHandler(context, _mockTimeProvider);

        var command = new CreateTenantCommand(
            Name: "FinGuard Global Ltd",
            UserName: "finguard_admin",
            PasswordHash: "secure_hash_123",
            Email: "dummyTest@email");

        // Act
        var tenantId = await handler.Handle(
            command,
            cancellationToken: TestContext.Current.CancellationToken);

        tenantId.Should().NotBeEmpty();

        TenantProvider.CurrentTenantId = tenantId;
        using var readContext = CreateDbContext();

        var savedTenant = await readContext.Tenants.FirstOrDefaultAsync(
            t => t.Id == tenantId,
            cancellationToken: TestContext.Current.CancellationToken);
        savedTenant.Should().NotBeNull();
        savedTenant.Name.Should().Be(command.Name);
        savedTenant.CreatedAt.Should().Be(_expectedTime.UtcDateTime);

        var savedUser = await readContext.Users.FirstOrDefaultAsync(
            cancellationToken: TestContext.Current.CancellationToken);
        savedUser.Should().NotBeNull();
        savedUser.UserName.Should().Be(command.UserName);
        savedUser.TenantId.Should().Be(tenantId);
        savedUser.PasswordHash.Should().Be(command.PasswordHash);
    }
}
