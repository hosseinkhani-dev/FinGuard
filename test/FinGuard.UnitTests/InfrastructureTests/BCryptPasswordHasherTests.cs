using FinGuard.Infrastructure.Security;
using FluentAssertions;

namespace FinGuard.UnitTests.InfrastructureTests;

public class BCryptPasswordHasherTests 
{
    [Fact]
    public void HashPassword_ShouldReturnValidHash_WhenPasswordIsProvided()
    {
        // Arrange
        var hasher = new BCryptPasswordHasher(workFactor: 4);
        var password = "SuperSecurePassword123!";

        // Act
        var hash = hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        var hasher = new BCryptPasswordHasher(workFactor: 4);
        var password = "SuperSecurePassword123!";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        var hasher = new BCryptPasswordHasher(workFactor: 4);
        var password = "SuperSecurePassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }
}
