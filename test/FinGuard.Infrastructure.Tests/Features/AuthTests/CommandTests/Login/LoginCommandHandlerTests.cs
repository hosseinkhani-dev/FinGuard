using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.Auth.Commands.Login;
using FinGuard.Domain.Entities;
using FinGuard.Infrastructure.Security;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace FinGuard.IntegrationTests.Features.AuthTests.CommandTests.Login;

public class LoginCommandHandlerTests : BaseIntegrationTest
{
    private readonly IJwtTokenGenerator _mockJwtTokenGenerator;
    private readonly BCryptPasswordHasher _passwordHasher;
    private readonly DateTimeOffset _fixedTime;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public LoginCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _mockJwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _passwordHasher = new BCryptPasswordHasher(4);
        _fixedTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _fakeTimeProvider = new FakeTimeProvider();
    }

    [Fact]
    public async Task Handle_ShouldReturnTokenAndRefreshToken_WhenCredentialsAreValid()
    {
        // Arrange
        string userName = "auth_user";
        string password = "Password123!";
        string expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";
        var tenantId = Guid.NewGuid();

        string hashedPassword = _passwordHasher.HashPassword(password);

        TenantProvider.CurrentTenantId = tenantId;

        using (var seedContext = CreateDbContext())
        {
            var user = new User(userName, hashedPassword, null);
            seedContext.Users.Add(user);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var context = CreateDbContext();
        _mockJwtTokenGenerator.GenerateToken(Arg.Any<User>()).Returns(expectedToken);

        _fakeTimeProvider.SetUtcNow(_fixedTime);

        var handler = new LoginCommandHandler(context, _mockJwtTokenGenerator, _passwordHasher, _fakeTimeProvider);
        var command = new LoginCommand(userName, password);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.AccessToken.Should().Be(expectedToken);
        result.RefreshToken.Should().NotBeNull();

        var expectedUser = await context.Users
            .IgnoreQueryFilters()
            .Include(u => u.RefreshTokens)
            .FirstAsync(u => u.UserName == userName,
            TestContext.Current.CancellationToken);

        expectedUser.Should().NotBeNull();
        expectedUser.RefreshTokens.Should().ContainSingle(r =>
        r.Token == result.RefreshToken &&
        r.CreatedAt == _fixedTime &&
        r.ExpiryTime == _fixedTime.AddDays(7) &&
       !r.IsRevoked
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new LoginCommandHandler(context, _mockJwtTokenGenerator, _passwordHasher, _fakeTimeProvider);
        var command = new LoginCommand("non_existent_user", "any_password");

        // Act
        var action = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Username or Password not exist!");
    }
}
