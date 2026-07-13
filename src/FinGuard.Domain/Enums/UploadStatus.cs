namespace FinGuard.Domain.Enums;

public enum UploadStatus : byte
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}
