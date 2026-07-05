using FinGuard.Application.Features.Tenants.Commands.CreateTenant;
using FluentValidation.TestHelper;

namespace FinGuard.IntegrationTests.Features.TenantTests.CommandTests.CreateTenant;

public class CreateTenantCommandValidatorTests 
{
    private readonly CreateTenantCommandValidator _validator;

    public CreateTenantCommandValidatorTests()
    {
        _validator = new CreateTenantCommandValidator();
    }

    [Fact]
    public void Validator_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateTenantCommand(
            "Valid Tenant", "admin_user", "secure_hash", "dummy@email");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldHaveError_WhenNameIsEmpty(string invalidName)
    {
        // Arrange
        var command = new CreateTenantCommand(invalidName, "admin_user", "secure_hash", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Tenant name cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenNameExceeds50Characters()
    {
        // Arrange
        var longName = new string('a', 51);
        var command = new CreateTenantCommand(longName, "admin_user", "secure_hash", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Tenant name should not be more than 50 character.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenUsernameIsEmpty()
    {
        // Arrange
        var command = new CreateTenantCommand("A Corp", "", "secure_hash", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
              .WithErrorMessage("UserName cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenUserNameExceeds50Characters()
    {
        // Arrange
        var longName = new string('a', 51);
        var command = new CreateTenantCommand("A Corp", longName, "secure_hash", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
              .WithErrorMessage("UserName cannot be more than 50 character.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailIsMissingAtSymbol()
    {
        // Arrange
        var command = new CreateTenantCommand(
            "Acme Corp", "admin", "hash", "invalid-email-format.com");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email address format.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailExceeds200Characters()
    {
        // Arrange
        var part1 = new string('a', 192);
        var longEmail = $"{part1}@test.com";

        var command = new CreateTenantCommand("Acme Corp", "admin", "hash", longEmail);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email cannot be more than 200 character.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenPasswordIsEmpty()
    {
        // Arrange
        var command = new CreateTenantCommand("Acme Corp", "admin", "", null);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password cannot be empty.");
    }
}
