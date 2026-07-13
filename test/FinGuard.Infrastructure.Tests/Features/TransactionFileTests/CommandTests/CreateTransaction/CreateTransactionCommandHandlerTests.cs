using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.TransactionFiles.Commands.CreateTransactionFile;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace FinGuard.IntegrationTests.Features.TransactionFileTests.CommandTests.CreateTransaction;

public class CreateTransactionCommandHandlerTests : BaseIntegrationTest
{
    private readonly ICurrentUser _currentUser;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly DateTimeOffset _fixedTime;
    private readonly Guid _currentUserId;

    public CreateTransactionCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _currentUserId = Guid.NewGuid();
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.UserId.Returns(_currentUserId);

        _fixedTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(_fixedTime);
    }

    [Fact]
    public async Task Handle_ShouldCreateTransactionFile_WhenRequestIsValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        using var context = CreateDbContext();
        TenantProvider.CurrentTenantId = tenantId;

        var handler = new CreateTransactionFileCommandHandler(context, _currentUser, _fakeTimeProvider);
        var command = new CreateTransactionFileCommand(
            OriginalFileName: "receipt.pdf",
            StoredFileName: "unique-guid-receipt.pdf",
            storagePath: "/storage/files/2026/",
            FileSize: 2048576);

        // Act
        Guid fileId = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        fileId.Should().NotBeEmpty();

        var expectedFile = await context.TransactionFiles.FirstAsync(f => f.Id == fileId,
            cancellationToken: TestContext.Current.CancellationToken);

        expectedFile.TenantId.Should().Be(tenantId);
        expectedFile.UploadedByUserId.Should().Be(_currentUserId);
        expectedFile.OriginalFileName.Should().Be(command.OriginalFileName);
        expectedFile.StoredFileName.Should().Be(command.StoredFileName);
        expectedFile.StoragePath.Should().Be(command.storagePath);
        expectedFile.FileSize.Should().Be(command.FileSize);
        expectedFile.CreatedAt.Should().Be(_fixedTime.UtcDateTime);
    }
}
