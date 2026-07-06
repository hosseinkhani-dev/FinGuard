using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Features.Tenants.Commands.CreateTenant;
using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Infrastructure.Security;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace FinGuard.IntegrationTests.Features.TenantTests.CommandTests.CreateTenant;

public class CreateTenantCommandHandlerTests : BaseIntegrationTest
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly DateTimeOffset _fixedTime;

    public CreateTenantCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _fixedTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _fakeTimeProvider = new FakeTimeProvider();
    }

    [Fact]
    public async Task Handle_ShouldRegisterTenantAndUser_WhenDataIsValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var passwordHasher = new BCryptPasswordHasher(4);
        _fakeTimeProvider.SetUtcNow(_fixedTime);

        var handler = new CreateTenantCommandHandler(context, _fakeTimeProvider, passwordHasher);

        var command = new CreateTenantCommand(
            Name: "FinGuard Global Ltd",
            UserName: "finguard_admin",
            Password: "secure_hash_123",
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
        savedTenant.CreatedAt.Should().Be(_fixedTime.UtcDateTime);

        var savedUser = await readContext.Users.FirstOrDefaultAsync(
            cancellationToken: TestContext.Current.CancellationToken);
        savedUser.Should().NotBeNull();
        savedUser.UserName.Should().Be(command.UserName);
        savedUser.TenantId.Should().Be(tenantId);
        savedUser.Role.Should().Be(UserRole.Admin);
        savedUser.Email!.EmailAddress.Should().Be(command.Email);
        passwordHasher.VerifyPassword(command.Password, savedUser.PasswordHash)
            .Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenUsernameExist()
    {
        // Arrange
        string userName = "dummy-user-name";

        TenantProvider.CurrentTenantId = Guid.NewGuid();
        using (var firstContext = CreateDbContext())
        {
            firstContext.Add(new User(userName, "qwe", UserRole.Admin, null));
            await firstContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var secondContext = CreateDbContext();
        var passwordHasher = new BCryptPasswordHasher(4);

        var handler = new CreateTenantCommandHandler(secondContext, _fakeTimeProvider, passwordHasher);

        var command = new CreateTenantCommand(
            Name: "FinGuard Global Ltd",
            UserName: userName,
            Password: "secure_hash_123",
            Email: "dummyTest@email");

        // Act
        var result = async () => 
        await handler.Handle(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
       await result.Should()
            .ThrowExactlyAsync<ConflictException>()
            .WithMessage($"This username {command.UserName} is already taken!");
    }
}
