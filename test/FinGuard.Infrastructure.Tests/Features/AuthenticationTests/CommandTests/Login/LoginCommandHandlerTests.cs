using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.Auth.Commands.Login;
using FinGuard.Domain.Entities;
using FinGuard.Infrastructure.Security;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using NSubstitute;

namespace FinGuard.IntegrationTests.Features.AuthenticationTests.CommandTests.Login;

public class LoginCommandHandlerTests : BaseIntegrationTest
{
    private readonly IJwtTokenGenerator _mockJwtTokenGenerator;
    private readonly BCryptPasswordHasher _passwordHasher;

    public LoginCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _mockJwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _passwordHasher = new BCryptPasswordHasher(4);
    }

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
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

        var handler = new LoginCommandHandler(context, _mockJwtTokenGenerator, _passwordHasher);
        var command = new LoginCommand(userName, password);

        // Act
        var token = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        token.Should().Be(expectedToken);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new LoginCommandHandler(context, _mockJwtTokenGenerator, _passwordHasher);
        var command = new LoginCommand("non_existent_user", "any_password");

        // Act
        var action = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Username or Password not exist!");
    }
}
