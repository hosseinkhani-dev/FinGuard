using FinGuard.Domain.Exceptions;

namespace FinGuard.Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public double VelocityThresholdMultiplier { get; private set; }
    public double ZScoreThreshold { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tenant() { }

    public Tenant( string name, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tenant name cannot be empty.");
        if (name.Length > 50)
            throw new DomainException("Tenant name should not be more than 50 character.");

        Id = Guid.NewGuid();
        Name = name;
        CreatedAt = timeProvider.GetUtcNow().UtcDateTime;

        // Default Thresholds
        VelocityThresholdMultiplier = 2.0; // 200%
        ZScoreThreshold = 3.0;
    }

    public void UpdateThresholds(
        double velocityThresholdMultiplier, double zScoreThreshold)
    {
        if (velocityThresholdMultiplier < 0)
            throw new DomainException("Velocity threshold cannot be negative.");

        if(zScoreThreshold < 0)
            throw new DomainException("ZScore threshold cannot be negative.");

        VelocityThresholdMultiplier = velocityThresholdMultiplier;
        ZScoreThreshold = zScoreThreshold;
    }
}
