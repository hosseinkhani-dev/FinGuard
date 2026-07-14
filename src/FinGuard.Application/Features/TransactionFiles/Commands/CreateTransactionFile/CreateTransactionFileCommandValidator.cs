using FluentValidation;

namespace FinGuard.Application.Features.TransactionFiles.Commands.CreateTransactionFile;

public class CreateTransactionFileCommandValidator : 
    AbstractValidator<CreateTransactionFileCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public CreateTransactionFileCommandValidator()
    {
        RuleFor(x => x.FileSize)
            .LessThan(MaxFileSize)
            .WithMessage("File size cannot be bigger than 10 MB");
    }
}
