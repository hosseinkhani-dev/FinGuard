using FluentValidation;

namespace FinGuard.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(u => u.UserName)
           .NotEmpty().WithMessage("UserName cannot be empty.")
           .MaximumLength(50).WithMessage("UserName cannot be more than 50 character.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password cannot be empty.");

        RuleFor(x => x.Email)
            .Must(email => email!.Contains("@"))
            .WithMessage("Invalid email address format.")
            .MaximumLength(200)
            .WithMessage("Email cannot be more than 200 character.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
