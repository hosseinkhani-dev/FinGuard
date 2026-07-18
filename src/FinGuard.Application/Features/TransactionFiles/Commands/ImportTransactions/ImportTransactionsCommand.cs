using MediatR;

namespace FinGuard.Application.Features.TransactionFiles.Commands.ImportTransactions;

public record ImportTransactionsCommand(
    Guid TransactionFileId) : IRequest<int>;
