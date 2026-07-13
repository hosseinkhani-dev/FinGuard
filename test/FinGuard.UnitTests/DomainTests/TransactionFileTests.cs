using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Domain.Exceptions;
using FluentAssertions;

namespace FinGuard.UnitTests.DomainTests;

public class TransactionFileTests
{
    private readonly DateTime _expectedCreatedAt;
    private readonly Guid _userId;
    private const string OriginalFileName = "transactions.xlsx";
    private const string StoredFileName = "stored_transactions.xlsx";
    private const string StoragePath = "storage/path/file.xlsx";
    private const long ValidFileSize = 5 * 1024 * 1024; // 5 MB

    public TransactionFileTests()
    {
        _expectedCreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _userId = Guid.NewGuid();
    }

    [Fact]
    public void Constructor_WithValidData_ShouldInitializeCorrectly()
    {
        // Act
        var transactionFile = new TransactionFile(
            _userId,
            OriginalFileName,
            StoredFileName,
            StoragePath,
            ValidFileSize,
            _expectedCreatedAt);

        // Assert
        transactionFile.Id.Should().NotBeEmpty();
        transactionFile.UploadedByUserId.Should().Be(_userId);
        transactionFile.OriginalFileName.Should().Be(OriginalFileName);
        transactionFile.StoredFileName.Should().Be(StoredFileName);
        transactionFile.StoragePath.Should().Be(StoragePath);
        transactionFile.FileSize.Should().Be(ValidFileSize);
        transactionFile.CreatedAt.Should().Be(_expectedCreatedAt);
        transactionFile.Format.Should().Be(TransactionFileFormat.Excel);
        transactionFile.Status.Should().Be(UploadStatus.Pending);
        transactionFile.ProcessingStartedAt.Should().BeNull();
        transactionFile.CompletedAt.Should().BeNull();
        transactionFile.FailedAt.Should().BeNull();
        transactionFile.FailureReason.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithFileSizeExceedingLimit_ShouldThrowDomainException()
    {
        // Arrange
        long invalidFileSize = 10485760 + 1; // 10MB + 1 byte

        // Act
        Action act = () => new TransactionFile(
            _userId,
            OriginalFileName,
            StoredFileName,
            StoragePath,
            invalidFileSize,
            _expectedCreatedAt);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("File size cannot be bigger than 10 MB");
    }

    [Fact]
    public void StartProcessing_WhenStatusIsPending_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var transactionFile = CreateValidTransactionFile();
        var startedAt = _expectedCreatedAt.AddMinutes(5);

        // Act
        transactionFile.StartProcessing(startedAt);

        // Assert
        transactionFile.Status.Should().Be(UploadStatus.Processing);
        transactionFile.ProcessingStartedAt.Should().Be(startedAt);
    }

    [Theory]
    [InlineData(UploadStatus.Processing)]
    [InlineData(UploadStatus.Completed)]
    [InlineData(UploadStatus.Failed)]
    public void StartProcessing_WhenStatusIsNotPending_ShouldThrowDomainException(UploadStatus currentStatus)
    {
        // Arrange
        var transactionFile = CreateValidTransactionFile();
        TransitionToStatus(transactionFile, currentStatus);
        var startedAt = _expectedCreatedAt.AddMinutes(5);

        // Act
        Action act = () => transactionFile.StartProcessing(startedAt);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Upload is not pending.");
    }

    [Fact]
    public void Complete_WhenStatusIsProcessing_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var transactionFile = CreateValidTransactionFile();
        transactionFile.StartProcessing(_expectedCreatedAt.AddMinutes(1));
        var completedAt = _expectedCreatedAt.AddMinutes(10);

        // Act
        transactionFile.Complete(completedAt);

        // Assert
        transactionFile.Status.Should().Be(UploadStatus.Completed);
        transactionFile.CompletedAt.Should().Be(completedAt);
    }

    [Theory]
    [InlineData(UploadStatus.Pending)]
    [InlineData(UploadStatus.Completed)]
    [InlineData(UploadStatus.Failed)]
    public void Complete_WhenStatusIsNotProcessing_ShouldThrowDomainException(UploadStatus currentStatus)
    {
        // Arrange
        var transactionFile = CreateValidTransactionFile();
        TransitionToStatus(transactionFile, currentStatus);
        var completedAt = _expectedCreatedAt.AddMinutes(10);

        // Act
        Action act = () => transactionFile.Complete(completedAt);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Upload is not processing.");
    }

    [Theory]
    [InlineData(UploadStatus.Pending)]
    [InlineData(UploadStatus.Processing)]
    [InlineData(UploadStatus.Failed)]
    public void Fail_WhenStatusIsNotCompleted_ShouldUpdateStatusAndFailureDetails(UploadStatus initialStatus)
    {
        // Arrange
        var transactionFile = CreateValidTransactionFile();
        TransitionToStatus(transactionFile, initialStatus);
        var failedAt = _expectedCreatedAt.AddMinutes(15);
        var reason = "Invalid file structure.";

        // Act
        transactionFile.Fail(failedAt, reason);

        // Assert
        transactionFile.Status.Should().Be(UploadStatus.Failed);
        transactionFile.FailedAt.Should().Be(failedAt);
        transactionFile.FailureReason.Should().Be(reason);
    }

    [Fact]
    public void Fail_WhenStatusIsCompleted_ShouldThrowDomainException()
    {
        // Arrange
        var transactionFile = CreateValidTransactionFile();
        TransitionToStatus(transactionFile, UploadStatus.Completed);
        var failedAt = _expectedCreatedAt.AddMinutes(15);

        // Act
        Action act = () => transactionFile.Fail(failedAt, "Some reason");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Completed upload cannot fail.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Fail_WithEmptyOrNullReason_ShouldThrowDomainException(string invalidReason)
    {
        // Arrange
        var transactionFile = CreateValidTransactionFile();
        var failedAt = _expectedCreatedAt.AddMinutes(15);

        // Act
        Action act = () => transactionFile.Fail(failedAt, invalidReason);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Failure reason cannot be empty.");
    }

    private TransactionFile CreateValidTransactionFile()
    {
        return new TransactionFile(
            _userId,
            OriginalFileName,
            StoredFileName,
            StoragePath,
            ValidFileSize,
            _expectedCreatedAt);
    }

    private void TransitionToStatus(TransactionFile file, UploadStatus targetStatus)
    {
        switch (targetStatus)
        {
            case UploadStatus.Processing:
                file.StartProcessing(_expectedCreatedAt.AddSeconds(1));
                break;
            case UploadStatus.Completed:
                file.StartProcessing(_expectedCreatedAt.AddSeconds(1));
                file.Complete(_expectedCreatedAt.AddSeconds(2));
                break;
            case UploadStatus.Failed:
                file.Fail(_expectedCreatedAt.AddSeconds(1), "Initial failure");
                break;
            case UploadStatus.Pending:
            default:
                break;
        }
    }
}
