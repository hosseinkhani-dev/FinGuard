using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Features.TransactionFiles.Commands.ImportTransactions;
using FinGuard.Application.Features.TransactionFiles.Commands.ProcessTransactionFile;
using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.IntegrationTests.Builders;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FinGuard.IntegrationTests.Features.TransactionFileTests.CommandTests.ProcessTransaction;

public class ProcessTransactionCommandHandlerTests : BaseIntegrationTest
{
    private readonly ISender _mediator;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly DateTimeOffset _fixedTime;

    public ProcessTransactionCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _mediator = Substitute.For<ISender>();
        _fixedTime = new DateTimeOffset(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(_fixedTime);
    }

    [Fact]
    public async Task Handle_ShouldCompleteProcessSuccessfully_WhenNoErrorsOccur()
    {
        // Arrange
        using var context = CreateDbContext();
        var tenantId = Guid.NewGuid();
        TenantProvider.CurrentTenantId = tenantId;

        var user = new UserBuilder().Build();
        user.AssignTenant(tenantId);

        var transactionFile = new TransactionFile(
            uploadedByUserId: user.Id,
            originalFileName: "to-process.xlsx",
            storedFileName: "stored-to-process.xlsx",
            storagePath: "uploads/to-process.xlsx",
            fileSize: 2048,
            _fixedTime.UtcDateTime
        );
        context.TransactionFiles.Add(transactionFile);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _mediator.Send(Arg.Any<ImportTransactionsCommand>(), Arg.Any<CancellationToken>())
            .Returns(15);

        var handler = new ProcessTransactionFileCommandHandler(context, _mediator, _fakeTimeProvider);
        var command = new ProcessTransactionFileCommand(transactionFile.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var updatedFile = await context.TransactionFiles
            .IgnoreQueryFilters()
            .FirstAsync(t => t.Id == transactionFile.Id, TestContext.Current.CancellationToken);

        updatedFile.Status.Should().Be(UploadStatus.Completed);
        updatedFile.ProcessingStartedAt.Should().Be(_fixedTime.UtcDateTime);
        updatedFile.CompletedAt.Should().Be(_fixedTime.UtcDateTime);

        // Verify Mediator call
        await _mediator.Received(1).Send(
            Arg.Is<ImportTransactionsCommand>(cmd => cmd.TransactionFileId == transactionFile.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTransactionFileNotFound()
    {
        // Arrange
        using var context = CreateDbContext();
        var nonExistentFileId = Guid.NewGuid();

        var handler = new ProcessTransactionFileCommandHandler(context, _mediator, _fakeTimeProvider);
        var command = new ProcessTransactionFileCommand(nonExistentFileId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Transaction file not found.");
    }

    [Fact]
    public async Task Handle_ShouldFailTransactionFileAndRethrow_WhenImportFails()
    {
        // Arrange
        using var context = CreateDbContext();
        var tenantId = Guid.NewGuid();
        TenantProvider.CurrentTenantId = tenantId;

        var user = new UserBuilder().Build();
        user.AssignTenant(tenantId);

        var transactionFile = new TransactionFile(
            uploadedByUserId: user.Id,
            originalFileName: "fail-process.xlsx",
            storedFileName: "stored-fail-process.xlsx",
            storagePath: "uploads/fail-process.xlsx",
            fileSize: 2048,
            _fixedTime.UtcDateTime
        );
        context.TransactionFiles.Add(transactionFile);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var expectedErrorMessage = "Invalid file structure.";
        _mediator.Send(Arg.Any<ImportTransactionsCommand>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException(expectedErrorMessage));

        var handler = new ProcessTransactionFileCommandHandler(context, _mediator, _fakeTimeProvider);
        var command = new ProcessTransactionFileCommand(transactionFile.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(expectedErrorMessage);

        var updatedFile = await context.TransactionFiles
            .IgnoreQueryFilters()
            .FirstAsync(t => t.Id == transactionFile.Id, TestContext.Current.CancellationToken);

        updatedFile.Status.Should().Be(UploadStatus.Failed);
        updatedFile.FailedAt.Should().Be(_fixedTime.UtcDateTime);
    }
}
