using MediatR;

namespace FinGuard.Application.Features.TransactionFiles.Commands.ProcessTransactionFile;

public record ProcessTransactionFileCommand(
    Guid TransactionFileId) : IRequest;
