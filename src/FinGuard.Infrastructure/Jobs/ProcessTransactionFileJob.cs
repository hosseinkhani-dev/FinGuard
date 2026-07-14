using FinGuard.Application.Features.TransactionFiles.Commands.ProcessTransactionFile;
using MediatR;

namespace FinGuard.Infrastructure.Jobs;

public class ProcessTransactionFileJob
{
    private readonly ISender _mediator;

    public ProcessTransactionFileJob(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute(Guid transactionFileId, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ProcessTransactionFileCommand(transactionFileId),
            cancellationToken);
    }
}
