using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Domain.Exceptions;
using FinGuard.Domain.ValueObjects;
using FluentAssertions;

namespace FinGuard.UnitTests.DomainTests;

public class UserTests
{
    private readonly DateTime _expectedCreatedAt;

    public UserTests()
    {
        _expectedCreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange
        var userName = "Dummy userName";
        var passwordHash = "dummy-password-hash";
        var email = new Email("dummy@email");

        // Act
        var user = new User( userName,
            passwordHash,
            UserRole.Auditor,
            email,
            _expectedCreatedAt);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.UserName.Should().Be(userName);
        user.PasswordHash.Should().Be(passwordHash);
        user.Email.Should().Be(email);
        user.Role.Should().Be(UserRole.Auditor);
        user.CreatedAt.Should().Be(_expectedCreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidUserName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange & Act
        Action act = () => new User( invalidName,
            "hash",
            UserRole.Admin,
            null,
            _expectedCreatedAt);

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
        Action act = () => new User( longName,
            "hash",
            UserRole.Admin,
            null,
            _expectedCreatedAt);

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

    [Fact]
    public void AssignTenant_WithEmptyId_ShouldThrowDomainException()
    {
        var invalidTenantId = Guid.Empty;
        var userName = "Dummy userName";
        var passwordHash = "dummy-password-hash";
        var email = new Email("dummy@email");

        var user = new User(userName, passwordHash, UserRole.Admin, email, _expectedCreatedAt);

        // Act 
        Action act = () => user.AssignTenant(invalidTenantId);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Tenant ID cannot be empty.");
    }

    [Fact]
    public void AssignTenant_WhenUserAlreadyHasTenant_ShouldThrowDomainException()
    {
        var tenantId = Guid.NewGuid();
        var userName = "Dummy userName";
        var passwordHash = "dummy-password-hash";
        var email = new Email("dummy@email");

        var user = new User(userName, passwordHash, UserRole.Admin, email, _expectedCreatedAt);

        user.AssignTenant(tenantId);

        // Act 
        Action act = () => user.AssignTenant(Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Cannot reassign a user to a different tenant.");
    }

    [Fact]
    public void Constructor_WithEmptyPassword_ShouldThrowDomainException()
    {
        var userName = "Dummy userName";
        var email = new Email("dummy@email");
        // Act 
        Action act = () => new User(userName, string.Empty, UserRole.Admin, email, _expectedCreatedAt);
        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Password cannot be empty.");
    }

    [Fact]
    public void Deactivate_AdminUser_ShouldThrowDomainException()
    {
        var userName = "Dummy userName";
        var passwordHash = "dummy-password-hash";
        var email = new Email("dummy@email");
        var adminUser = new User(userName, passwordHash, UserRole.Admin, email, _expectedCreatedAt);

        // Act
        Action act = () => adminUser.Deactivate();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Admin user cannot be deactivated.");
    }
}
