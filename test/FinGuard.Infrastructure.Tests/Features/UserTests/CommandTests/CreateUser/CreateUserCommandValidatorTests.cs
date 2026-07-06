using FinGuard.Application.Features.Users.Commands.CreateUser;
using FluentValidation.TestHelper;

namespace FinGuard.IntegrationTests.Features.UserTests.CommandTests.CreateUser;

public class CreateUserCommandValidatorTests 
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    [Fact]
    public void Validator_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateUserCommand(
            "ValidUser", "secure_password", "dummy@email");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenUserNameIsEmpty()
    {
        // Arrange
        var command = new CreateUserCommand("", "secure_password", null);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
              .WithErrorMessage("UserName cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenUserNameExceedsMaxLength()
    {
        // Arrange
        var command = new CreateUserCommand(new string('a', 51), "secure_password", null);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
              .WithErrorMessage("UserName cannot be more than 50 character.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenPasswordIsEmpty()
    {
        // Arrange
        var command = new CreateUserCommand("ValidUser", "", null);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new CreateUserCommand("ValidUser", "secure_password", "invalidemail");
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email address format.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailExceedsMaxLength()
    {
        // Arrange
        var command = new CreateUserCommand("ValidUser", "secure_password", new string('a', 201) + "@email.com");
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email cannot be more than 200 character.");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenEmailIsNull()
    {
        // Arrange
        var command = new CreateUserCommand("ValidUser", "secure_password", null);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
