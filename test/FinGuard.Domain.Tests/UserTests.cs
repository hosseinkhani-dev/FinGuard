using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Domain.Exceptions;
using FinGuard.Domain.ValueObjects;
using FluentAssertions;

namespace FinGuard.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userName = "Dummy userName";
        var passwordHash = "dummy-password-hash";
        var email = new Email("dummy@email");

        // Act
        var user = new User(tenantId, userName, passwordHash, email);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.UserName.Should().Be(userName);
        user.TenantId.Should().Be(tenantId);
        user.PasswordHash.Should().Be(passwordHash);
        user.Email.Should().Be(email);
        user.Role.Should().Be(UserRole.Auditor);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ShouldThrowDomainException()
    {
        // Arrange
        var invalidTenantId = Guid.Empty;

        // Act
        Action act = () => new User(invalidTenantId, "valid_user", "hash", null);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Tenant Id cannot be null.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidUserName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange & Act
        Action act = () => new User(Guid.NewGuid(), invalidName, "hash", null);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("UserName cannot be empty.");
    }

    [Fact]
    public void Constructor_WithUserNameTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var longName = new string('a', 51);

        // Act
        Action act = () => new User(Guid.NewGuid(), longName, "hash", null);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("UserName cannot be more than 50 character.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalidEmail")]
    public void Constructor_WithInvalidEmail_ShouldThrowDomainException(string invalidEmail)
    {
        // Arrange & Act
        Action act = () => new Email(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Invalid email address format.");
    }
}
