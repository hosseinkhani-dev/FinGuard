using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Features.Users.Commands.ActivateUser;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.IntegrationTests.Builders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.IntegrationTests.Features.UserTests.CommandTests.ActivateUser;

public class ActivateUserCommandHandlerTests : BaseIntegrationTest
{
    public ActivateUserCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Handle_ShouldActivateUser_WhenUserExists()
    {
        // Arrange
        using var context = CreateDbContext();

        var user = new UserBuilder().Build();
        user.Deactivate();
        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new ActivateUserCommandHandler(context);
        var command = new ActivateUserCommand(user.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var expectedUser = await context.Users.SingleAsync(TestContext.Current.CancellationToken);
        expectedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new ActivateUserCommandHandler(context);
        var command = new ActivateUserCommand(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }
}
