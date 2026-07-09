using FinGuard.Application.Features.Users.Queries.GetAllUsers;
using FinGuard.Domain.Enums;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.IntegrationTests.Builders;
using FluentAssertions;

namespace FinGuard.IntegrationTests.Features.UserTests.QueryTests.GetAllUsers;

public class GetAllUsersQueryHandlerTests : BaseIntegrationTest
{
    public GetAllUsersQueryHandlerTests(DbTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUsers_WhenNoFiltersAreApplied()
    {
        // Arrange
        using var context = CreateDbContext();

        var user1 = new UserBuilder()
            .WithUserName("testusername1")
            .WithPassword("testpassword1")
            .WithEmail(null)
            .Build();
        var user2 = new UserBuilder()
            .WithUserName("testusername2")
            .WithPassword("testpassword2")
            .WithRole(UserRole.Auditor)
            .WithEmail("test2@email")
            .Build();
        user2.Deactivate();
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersQueryHandler(context);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(user2.Id);
        result[0].UserName.Should().Be(user2.UserName);
        result[0].Email.Should().Be(user2.Email!.EmailAddress);
        result[0].ActiveStatus.Should().Be("Inactive");

        result[1].Id.Should().Be(user1.Id);
        result[1].UserName.Should().Be(user1.UserName);
        result[1].Email.Should().BeNull();
        result[1].ActiveStatus.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredUsers_WhenNameFiltersApplied()
    {
        // Arrange
        using var context = CreateDbContext();
        var user1 = new UserBuilder()
            .WithUserName("testusername1")
            .Build();
        var user2 = new UserBuilder()
            .WithUserName("testusername2")
            .Build();
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var query = new GetAllUsersQuery(UserName: "testusername1");
        var handler = new GetAllUsersQueryHandler(context);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(user1.Id);
        result[0].UserName.Should().Be(user1.UserName);
        result[0].Email.Should().BeNull();
        result[0].ActiveStatus.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredUsers_WhenEmailFiltersApplied()
    {
        // Arrange
        using var context = CreateDbContext();
        var user1 = new UserBuilder()
            .WithUserName("testusername1")
            .WithEmail("test1@email")
            .Build();
        var user2 = new UserBuilder()
            .WithUserName("testusername2")
            .WithEmail("test2@email")
            .Build();
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var query = new GetAllUsersQuery(Email: "test1@email");
        var handler = new GetAllUsersQueryHandler(context);
        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);
        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(user1.Id);
        result[0].UserName.Should().Be(user1.UserName);
        result[0].Email.Should().Be(user1.Email!.EmailAddress);
        result[0].ActiveStatus.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredUsers_WhenIsActiveFiltersApplied()
    {
        // Arrange
        using var context = CreateDbContext();
        var user1 = new UserBuilder()
            .WithUserName("testusername1")
            .Build();
        var user2 = new UserBuilder()
            .WithUserName("testusername2")
            .WithRole(UserRole.Auditor)
            .Build();
        user2.Deactivate();
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var query = new GetAllUsersQuery(IsActive: true);
        var handler = new GetAllUsersQueryHandler(context);
        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);
        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(user1.Id);
        result[0].UserName.Should().Be(user1.UserName);
        result[0].Email.Should().BeNull();
        result[0].ActiveStatus.Should().Be("Active");
    }
}
