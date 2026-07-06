using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Features.Users.Commands.DisableUser;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.IntegrationTests.Builders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.IntegrationTests.Features.UserTests.CommandTests.DisableUser;

public class DisableUserCommandHandlerTests : BaseIntegrationTest
{
    public DisableUserCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Handle_ShouldDisableUser_WhenUserExists()
    {
        // Arrange
        using var context = CreateDbContext();

        var user = new UserBuilder().Build();
        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DisableUserCommandHandler(context);
        var command = new DisableUserCommand(user.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var expectedUser = await context.Users.SingleAsync(TestContext.Current.CancellationToken);

        expectedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new DisableUserCommandHandler(context);
        var command = new DisableUserCommand(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }
}
