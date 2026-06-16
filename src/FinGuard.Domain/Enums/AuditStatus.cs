namespace FinGuard.Domain.Enums;

public enum AuditStatus : byte
{
    Clean = 1,
    Flagged = 2,
    Approved = 3,
    Dismissed = 4
}
