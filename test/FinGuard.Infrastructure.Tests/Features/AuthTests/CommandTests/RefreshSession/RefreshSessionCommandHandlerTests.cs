using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.Auth.Commands.RefreshSessions;
using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace FinGuard.IntegrationTests.Features.AuthTests.CommandTests.RefreshSession;

public class RefreshSessionCommandHandlerTests : BaseIntegrationTest
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly IJwtTokenGenerator _mockJwtTokenGenerator;
    private readonly DateTime _fixedTime;

    public RefreshSessionCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _mockJwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _mockJwtTokenGenerator.GenerateToken(Arg.Any<User>())
            .Returns("secure-jwt-token");
        _fixedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(_fixedTime);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewAccessTokenAndRefreshToken_WhenCredentialsAreValid()
    {
        // Arrange
        string initialRefreshToken = "seed-token-string-12345";

        TenantProvider.CurrentTenantId = Guid.NewGuid();
        using var context = CreateDbContext();

        var user = new User("dummy-username", "securePassword", UserRole.Admin, null);
        user.AddRefreshToken(initialRefreshToken, _fixedTime, _fixedTime.AddDays(7));
        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new RefreshSessionCommandHandler(context, _fakeTimeProvider, _mockJwtTokenGenerator);
        var command = new RefreshSessionCommand(initialRefreshToken);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();

        result.AccessToken.Should().Be("secure-jwt-token");
        result.RefreshToken.Should().NotBeEmpty();

        var expectedUser = await context.Users
            .FirstAsync(u => u.UserName == user.UserName,
            TestContext.Current.CancellationToken);

        expectedUser.RefreshTokens.Should().ContainSingle(r =>
        r.Token == result.RefreshToken &&
        r.ExpiryTime == _fixedTime.AddDays(7) &&
        r.CreatedAt == _fixedTime &&
        !r.IsRevoked
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserWithGivenTokenNotExist()
    {
        // Arrange
        string initialRefreshToken = "seed-token-string-12345";

        TenantProvider.CurrentTenantId = Guid.NewGuid();
        using var context = CreateDbContext();

        var handler = new RefreshSessionCommandHandler(context, _fakeTimeProvider, _mockJwtTokenGenerator);
        var command = new RefreshSessionCommand(initialRefreshToken);

        // Act
        var action = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid session.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenGivenRefreshTokenIsExpired()
    {
        // Arrange
        string initialRefreshToken = "seed-token-string-12345";

        TenantProvider.CurrentTenantId = Guid.NewGuid();
        using var context = CreateDbContext();

        var user = new User("dummy-username", "securePassword", UserRole.Admin, null);
        user.AddRefreshToken(initialRefreshToken, _fixedTime, _fixedTime.AddHours(1));
        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _fakeTimeProvider.SetUtcNow(_fixedTime.AddHours(1));
        var handler = new RefreshSessionCommandHandler(context, _fakeTimeProvider, _mockJwtTokenGenerator);
        var command = new RefreshSessionCommand(initialRefreshToken);

        // Act
        var action = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Session has expired or been revoked*");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenGivenRefreshTokenIsRevoked()
    {
        // Arrange
        string initialRefreshToken = "seed-token-string-12345";

        TenantProvider.CurrentTenantId = Guid.NewGuid();
        using var context = CreateDbContext();

        var user = new User("dummy-username", "securePassword", UserRole.Admin, null);
        user.AddRefreshToken(initialRefreshToken, _fixedTime, _fixedTime.AddHours(1));
        var refreshToken = user.RefreshTokens.First();
        refreshToken.Revoke();
        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new RefreshSessionCommandHandler(context, _fakeTimeProvider, _mockJwtTokenGenerator);
        var command = new RefreshSessionCommand(initialRefreshToken);

        // Act
        var action = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Session has expired or been revoked*");
    }
}
