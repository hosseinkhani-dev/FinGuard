using FinGuard.Domain.Entities;
using FinGuard.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace FinGuard.Domain.Tests;

public class TenantTests
{
    private readonly FakeTimeProvider _fakeClock;
    public TenantTests()
    {
        _fakeClock = new FakeTimeProvider();
        _fakeClock.SetUtcNow(new DateTime(2026, 01,01));
    }

    [Fact]
    public void Constructor_WithValidName_ShouldInitializeWithDefaultThresholds()
    {
        // Arrange
        var defaultVelocityThreshold = 2.0;
        var defaultZScoreThreshold = 3.0;

        var name = "Dummy tenant name";

        // Act
        var tenant = new Tenant(name, _fakeClock);

        // Assert
        tenant.Id.Should().NotBeEmpty();
        tenant.Name.Should().Be(name);
        tenant.CreatedAt.Should().Be(_fakeClock.GetUtcNow().UtcDateTime);
        tenant.VelocityThresholdMultiplier.Should().Be(defaultVelocityThreshold);
        tenant.ZScoreThreshold.Should().Be(defaultZScoreThreshold);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithEmptyName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange & Act
        Action act = () => new Tenant(invalidName, _fakeClock);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Tenant name cannot be empty.");
            
    }

    [Fact]
    public void Constructor_WithInvalidCharacterName_ShouldThrowDomainException()
    {
        // Arrange & Act
        Action act = () => new Tenant(new string('a', 51), _fakeClock);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Tenant name should not be more than 50 character.");
    }

    [Fact]
    public void UpdateThresholds_WithValidValues_ShouldUpdateProperties()
    {
        // Arrange
        var tenant = new Tenant("Dummy tenant name", _fakeClock);
        double newVelocityMultiplier = 3.5;
        double newZScore = 2.5;

        // Act
        tenant.UpdateThresholds(newVelocityMultiplier, newZScore);

        // Assert
        tenant.VelocityThresholdMultiplier.Should().Be(newVelocityMultiplier);
        tenant.ZScoreThreshold.Should().Be(newZScore);
    }

    [Fact]
    public void UpdateThresholds_WithNegativeVelocityMultiplier_ShouldThrowDomainException()
    {
        // Arrange
        var tenant = new Tenant("Dummy tenant name", _fakeClock);
        double invalidVelocityMultiplier = -1.5;
        double validZScore = 3.0;

        // Act
        Action act = () => tenant.UpdateThresholds(invalidVelocityMultiplier, validZScore);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Velocity threshold cannot be negative.");
    }

    [Fact]
    public void UpdateThresholds_WithNegativeZScore_ShouldThrowDomainException()
    {
        // Arrange
        var tenant = new Tenant("Dummy tenant name", _fakeClock);
        double validVelocityMultiplier = 2.0;
        double invalidZScore = -0.5;

        // Act
        Action act = () => tenant.UpdateThresholds(validVelocityMultiplier, invalidZScore);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("ZScore threshold cannot be negative.");
    }
}
