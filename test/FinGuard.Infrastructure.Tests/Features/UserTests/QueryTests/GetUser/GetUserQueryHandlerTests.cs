using FinGuard.Application.Features.Users.Queries.GetUser;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.IntegrationTests.Builders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.IntegrationTests.Features.UserTests.QueryTests.GetUser;

public class GetUserQueryHandlerTests : BaseIntegrationTest
{
    public GetUserQueryHandlerTests(DbTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserExists()
    {
        // Arrange
        using var context = CreateDbContext();

        var user = new UserBuilder()
            .WithEmail("test@email")
            .Build();

        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetUserQuery(user.Id);
        var handler = new GetUserQueryHandler(context);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.UserName.Should().Be(user.UserName);
        result.Email.Should().Be(user.Email!.EmailAddress);
        result.ActiveStatus.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNoUsersExist()
    {
        // Arrange
        using var context = CreateDbContext();

        await context.Users.ExecuteDeleteAsync(TestContext.Current.CancellationToken);

        var query = new GetUserQuery(Guid.NewGuid());
        var handler = new GetUserQueryHandler(context);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }
}
