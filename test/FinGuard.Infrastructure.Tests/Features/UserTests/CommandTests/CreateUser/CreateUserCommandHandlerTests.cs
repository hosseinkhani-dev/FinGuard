using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.Users.Commands.CreateUser;
using FinGuard.Domain.Enums;
using FinGuard.Domain.ValueObjects;
using FinGuard.Infrastructure.Security;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.IntegrationTests.Builders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace FinGuard.IntegrationTests.Features.UserTests.CommandTests.CreateUser;

public class CreateUserCommandHandlerTests : BaseIntegrationTest
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly DateTimeOffset _fixedTime;

    public CreateUserCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _passwordHasher = new BCryptPasswordHasher(4);
        _fixedTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(_fixedTime);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenCredentialsAreValid()
    {
        // Arrange
        using var context = CreateDbContext();

        var tenant = new TenantBuilder().Build();
        context.Tenants.Add(tenant);

        var admin = new UserBuilder().Build();
        admin.AssignTenant(tenant.Id);
        context.Users.Add(admin);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        TenantProvider.CurrentTenantId = tenant.Id;
        Email newUserEmail = new Email("newUser@email");
        var handler = new CreateUserCommandHandler(context, _passwordHasher, _fakeTimeProvider);
        var command = new CreateUserCommand(
            UserName: "newUser",
            Password: "123",
            Email: newUserEmail.EmailAddress);

        // Act
        Guid newUserId = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        newUserId.Should().NotBeEmpty();

        var expectedUser = await context.Users.FirstAsync(u => u.Id == newUserId,
            cancellationToken: TestContext.Current.CancellationToken);
        expectedUser.TenantId.Should().Be(tenant.Id);
        expectedUser.UserName.Should().Be(command.UserName);
        expectedUser.Email!.EmailAddress.Should().Be(command.Email);
        expectedUser.Role.Should().Be(UserRole.Auditor);
        expectedUser.CreatedAt.Should().Be(_fixedTime.UtcDateTime);
        _passwordHasher.VerifyPassword(
        command.Password, expectedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenUsernameExists()
    {
        // Arrange
        TenantProvider.CurrentTenantId = Guid.NewGuid();
        using var context = CreateDbContext();

        var existingUser = new UserBuilder()
            .WithUserName("existingUser")
            .WithPassword("password")
            .WithRole(UserRole.Auditor)
            .Build();
        context.Users.Add(existingUser);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateUserCommandHandler(context, _passwordHasher, _fakeTimeProvider);
        var command = new CreateUserCommand(
            UserName: existingUser.UserName,
            Password: "newPassword");

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"User with username '{existingUser.UserName}' already exists.");
    }
}