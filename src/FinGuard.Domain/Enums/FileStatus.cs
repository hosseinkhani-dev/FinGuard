namespace FinGuard.Domain.Enums;

public enum FileStatus : byte
{
    Uploaded = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}
