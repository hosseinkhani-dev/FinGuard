using MediatR;

namespace FinGuard.Application.Features.TransactionFiles.Commands.CreateTransactionFile;

public record CreateTransactionFileCommand(
   string OriginalFileName,
   string StoredFileName,
   string storagePath,
   long FileSize) : IRequest<Guid>;
