using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.TransactionFiles.Commands.ImportTransactions;
using FinGuard.Application.Features.TransactionFiles.DTOs;
using FinGuard.Domain.Entities;
using FinGuard.Infrastructure.Tests;
using FinGuard.Infrastructure.Tests.Fixtures;
using FinGuard.IntegrationTests.Builders;
using FinGuard.IntegrationTests.Builders.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FinGuard.IntegrationTests.Features.TransactionFileTests.CommandTests.ImportTransaction;

public class ImportTransactionCommandHandlerTests : BaseIntegrationTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITransactionFileReader _fileReader;
    private readonly DateTime _fixedDateTime;

    public ImportTransactionCommandHandlerTests(DbTestFixture fixture) : base(fixture)
    {
        _fileStorage = Substitute.For<IFileStorage>();
        _fileReader = Substitute.For<ITransactionFileReader>();
        _fixedDateTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public async Task Handle_ShouldImportTransactionsInBatches_WhenFileAndDataAreValid()
    {
        // Arrange
        using var context = CreateDbContext();

        var tenantId = Guid.NewGuid();
        TenantProvider.CurrentTenantId = tenantId;
        var user = new UserBuilder().Build();
        user.AssignTenant(tenantId);

        var transactionFile = new TransactionFile(
            uploadedByUserId: user.Id,
            originalFileName: "transactions.xlsx",
            storedFileName: "stored-transactions.xlsx",
            storagePath: "uploads/transactions.xlsx",
            fileSize: 1024,
            _fixedDateTime
        );
        context.TransactionFiles.Add(transactionFile);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // 2. Setup mock file stream and reader payload
        var dummyStream = new MemoryStream();
        _fileStorage.OpenReadAsync(transactionFile.StoragePath, Arg.Any<CancellationToken>())
            .Returns(dummyStream);

        var importedRows = new List<ExcelTransactionImportRowDto>
        {
            new()
            {
                EmployeeName = "John Doe",
                Department = "Engineering",
                TransactionDate = new DateTime(2026, 7, 1, 1, 1, 1, DateTimeKind.Utc),
                CardNumber = "1234-5678",
                Amount = 150.50m,
                Merchant = "AWS Cloud Services",
                Currency = "USD",
                Description = "Hosting services"
            },
            new()
            {
                EmployeeName = "Jane Smith",
                Department = "Marketing",
                TransactionDate = new DateTime(2026, 7, 2 , 1, 1, 1, DateTimeKind.Utc),
                CardNumber = "8765-4321",
                Amount = 45.00m,
                Merchant = "SaaS Tool",
                Currency = "EUR",
                Description = "Monthly subscription"
            }
        };

        _fileReader.ReadAsync(dummyStream, Arg.Any<CancellationToken>())
            .Returns(importedRows);

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(transactionFile.Id);

        // Act
        int result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(2);

        var insertedTransactions = await context.Transactions
            .Where(t => t.TransactionFileId == transactionFile.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        insertedTransactions.Should().HaveCount(2);

        // Verify first record mappings
        var first = insertedTransactions.Single(t => t.EmployeeName == "John Doe");
        first.TenantId.Should().Be(tenantId);
        first.TransactionFileId.Should().Be(transactionFile.Id);
        first.Department.Should().Be(importedRows[0].Department);
        first.TransactionDate.Should().Be(importedRows[0].TransactionDate);
        first.Amount.Should().Be(importedRows[0].Amount);
        first.Merchant.Should().Be(importedRows[0].Merchant);
        first.Currency.Should().Be(importedRows[0].Currency);
        first.Description.Should().Be(importedRows[0].Description);

        var second = insertedTransactions.Single(t => t.EmployeeName == "Jane Smith");
        second.TenantId.Should().Be(tenantId);
        second.TransactionFileId.Should().Be(transactionFile.Id);
        second.Department.Should().Be(importedRows[1].Department);
        second.TransactionDate.Should().Be(importedRows[1].TransactionDate);
        second.Amount.Should().Be(importedRows[1].Amount);
        second.Merchant.Should().Be(importedRows[1].Merchant);
        second.Currency.Should().Be(importedRows[1].Currency);
        second.Description.Should().Be(importedRows[1].Description);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTransactionFileNotFound()
    {
        // Arrange
        using var context = CreateDbContext();
        var nonExistentFileId = Guid.NewGuid();

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(nonExistentFileId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Transaction file not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmployeeNameIsMissing()
    {
        // Arrange
        using var context = CreateDbContext();

        var importDto = new ExcelTransactionImportRowBuilder()
            .WithEmployeeName("")
            .Build();
        var file = await SeedTransactionFileAsync(context);
        SetupFileReaderWithSingleRow(file, importDto);

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(file.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ImportValidationException>()
            .WithMessage("Row 2: Employee name is required.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDepartmentIsMissing()
    {
        // Arrange
        using var context = CreateDbContext();
        var file = await SeedTransactionFileAsync(context);
        var importDto = new ExcelTransactionImportRowBuilder()
           .WithDepartment("")
           .Build();
        SetupFileReaderWithSingleRow(file, importDto);

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(file.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ImportValidationException>()
            .WithMessage("Row 2: Department is required.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        using var context = CreateDbContext();
        var file = await SeedTransactionFileAsync(context); 
        var importDto = new ExcelTransactionImportRowBuilder()
            .WithAmount(0)
            .Build();
        SetupFileReaderWithSingleRow(file, importDto);

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(file.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ImportValidationException>()
            .WithMessage("Row 2: Amount must be greater than zero.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenMerchantIsMissing()
    {
        // Arrange
        using var context = CreateDbContext();
        var file = await SeedTransactionFileAsync(context);
        var importDto = new ExcelTransactionImportRowBuilder()
            .WithMerchant("")
            .Build();
        SetupFileReaderWithSingleRow(file, importDto);

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(file.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ImportValidationException>()
            .WithMessage("Row 2: Merchant is required.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCurrencyIsMissing()
    {
        // Arrange
        using var context = CreateDbContext();
        var file = await SeedTransactionFileAsync(context);
        var importDto = new ExcelTransactionImportRowBuilder()
            .WithCurrency("")
            .Build();
        SetupFileReaderWithSingleRow(file, importDto);

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(file.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ImportValidationException>()
            .WithMessage("Row 2: Currency is required.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTransactionDateIsDefault()
    {
        // Arrange
        using var context = CreateDbContext();
        var file = await SeedTransactionFileAsync(context);
        var importDto = new ExcelTransactionImportRowBuilder()
            .WithTransactionDate(default)
            .Build();
        SetupFileReaderWithSingleRow(file, importDto);

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(file.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ImportValidationException>()
            .WithMessage("Row 2: Transaction date is required.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDescriptionExceedsLimit()
    {
        // Arrange
        using var context = CreateDbContext();
        var file = await SeedTransactionFileAsync(context);
        var importDto = new ExcelTransactionImportRowBuilder()
            .WithDescription(new string('x', 501))
            .Build();
        SetupFileReaderWithSingleRow(file, importDto);

        var handler = new ImportTransactionsCommandHandler(context, _fileStorage, _fileReader);
        var command = new ImportTransactionsCommand(file.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ImportValidationException>()
            .WithMessage("Row 2: Description cannot exceed 500 characters.");
    }

    private async Task<TransactionFile> SeedTransactionFileAsync(IFinGuardDbContext context)
    {
        var tenantId = Guid.NewGuid();
        TenantProvider.CurrentTenantId = tenantId;

        var user = new UserBuilder().Build();
        user.AssignTenant(tenantId);

        var transactionFile = new TransactionFile(
            uploadedByUserId: user.Id,
            originalFileName: "validation.xlsx",
            storedFileName: "stored-validation.xlsx",
            storagePath: "uploads/validation.xlsx",
            fileSize: 1024,
            _fixedDateTime
        );

        context.TransactionFiles.Add(transactionFile);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return transactionFile;
    }

    private void SetupFileReaderWithSingleRow(TransactionFile file, ExcelTransactionImportRowDto row)
    {
        var dummyStream = new MemoryStream();
        _fileStorage.OpenReadAsync(file.StoragePath, Arg.Any<CancellationToken>())
            .Returns(dummyStream);

        _fileReader.ReadAsync(dummyStream, Arg.Any<CancellationToken>())
            .Returns(new List<ExcelTransactionImportRowDto> { row });
    }
}
