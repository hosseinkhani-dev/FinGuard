namespace FinGuard.Domain.Enums;

public enum AnomalyType : byte
{
    None = 1,
    DuplicatedInvoicing = 2,
    VelocitySpike = 3,
    StatisticalAnomaly =4
}
