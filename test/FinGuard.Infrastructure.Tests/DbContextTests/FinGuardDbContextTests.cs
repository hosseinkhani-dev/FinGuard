using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.IntegrationTests.Builders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Infrastructure.Tests.DbContextTests;

public class FinGuardDbContextTests : BaseIntegrationTest
{
    public FinGuardDbContextTests(DbTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldAutomaticallyAssignTenantId_WhenEntityIsAdded()
    {
        // Arrange
        var expectedTenantId = Guid.NewGuid();
        TenantProvider.CurrentTenantId = expectedTenantId;

        using var context = CreateDbContext();

        // Act
        var newUser = new UserBuilder()
            .WithUserName("dummy-user-name")
            .WithPassword("dummy-password")
            .WithRole(UserRole.Admin)
            .WithEmail(string.Empty)
            .Build();
        context.Users.Add(newUser);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedUser = await context.Users.IgnoreQueryFilters()
            .FirstAsync(cancellationToken: TestContext.Current.CancellationToken);
        savedUser.Id.Should().NotBeEmpty();
        savedUser.Email.Should().BeNull();
        savedUser.UserName.Should().Be(newUser.UserName);
        savedUser.PasswordHash.Should().Be(newUser.PasswordHash);
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldOnlyReturnData_BelongingToCurrentTenant()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        TenantProvider.CurrentTenantId = tenant1Id;
        using (var context = CreateDbContext())
        {
            var user = new UserBuilder()
           .WithUserName("first-user-name")
           .WithPassword("first-user-pass")
           .WithRole(UserRole.Admin)
           .WithEmail(null)
           .Build();
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        TenantProvider.CurrentTenantId = tenant2Id;
        using (var context = CreateDbContext())
        {
            var user = new UserBuilder()
                .WithUserName("second-user-name")
                .WithPassword("second-pass-user")
                .WithRole(UserRole.Admin)
                .WithEmail(null)
                .Build();
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        TenantProvider.CurrentTenantId = tenant1Id;
        using var readContext = CreateDbContext();
        var visibleUser = await readContext.Users.ToListAsync(
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        visibleUser.Should().HaveCount(1);
        var expectedUser = visibleUser.Single();
        expectedUser.TenantId.Should().Be(tenant1Id);
        expectedUser.UserName.Should().Be("first-user-name");
    }
}
