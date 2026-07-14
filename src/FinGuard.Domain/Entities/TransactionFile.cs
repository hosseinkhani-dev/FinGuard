using FinGuard.Domain.Common;
using FinGuard.Domain.Enums;
using FinGuard.Domain.Exceptions;

namespace FinGuard.Domain.Entities;

public class TransactionFile : ITenant
{
    public Guid Id { get; private set; }
    public Guid TenantId {  get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public string OriginalFileName { get; private set; } = null!;
    public string StoredFileName { get; private set; } = null!;
    public string StoragePath { get; private set; } = null!;
    public long FileSize { get; private set; }
    public UploadStatus Status { get; private set; }
    public TransactionFileFormat Format { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessingStartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }

    // Navigation Properties
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions =>
        _transactions.AsReadOnly();

    private TransactionFile() { }

    public TransactionFile(
        Guid uploadedByUserId,
        string originalFileName,
        string storedFileName,
        string storagePath,
        long fileSize,
        DateTime createdAt)
    {
        if(fileSize > (10 * 1024 * 1024)) // 10MB
            throw new DomainException("File size cannot be bigger than 10 MB");

        if (originalFileName.Length > 50 || storedFileName.Length > 50)
            throw new DomainException("file name cannot be more than 50 character");


        Id = Guid.NewGuid();
        UploadedByUserId = uploadedByUserId;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        StoragePath = storagePath;
        FileSize = fileSize;
        CreatedAt = createdAt;
        Format = TransactionFileFormat.Excel;

        Status = UploadStatus.Pending;
    }

    public void StartProcessing(DateTime startedAt)
    {
        Status = UploadStatus.Processing;
        ProcessingStartedAt = startedAt;
    }

    public void Complete(DateTime completedAt)
    {
        if(Status != UploadStatus.Processing)
            throw new DomainException("Upload is not processing.");

        Status = UploadStatus.Completed;
        CompletedAt = completedAt;
    }

    public void Fail(DateTime failedDate, string reason)
    {
        if (Status == UploadStatus.Completed)
            throw new DomainException("Completed upload cannot fail.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Failure reason cannot be empty.");

        Status = UploadStatus.Failed;
        FailedAt = failedDate;
        FailureReason = reason;
    }

    public void ResetToPending()
    {
        if (Status != UploadStatus.Processing)
            throw new DomainException("Upload is not processing.");

        Status = UploadStatus.Pending;
        ProcessingStartedAt = null;
    }
}
