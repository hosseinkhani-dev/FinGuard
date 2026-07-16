using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.TransactionFiles.Commands.ImportTransactions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.TransactionFiles.Commands.ProcessTransactionFile;

public class ProcessTransactionFileCommandHandler :
    IRequestHandler<ProcessTransactionFileCommand>
{
    private readonly IFinGuardDbContext _context;
    private readonly ISender _mediator;
    private readonly TimeProvider _timeProvider;

    public ProcessTransactionFileCommandHandler(
        IFinGuardDbContext context,
        ISender mediator,
        TimeProvider timeProvider)
    {
        _context = context;
        _mediator = mediator;
        _timeProvider = timeProvider;
    }

    public async Task Handle(
        ProcessTransactionFileCommand request,
        CancellationToken cancellationToken)
    {
        var transactionFile = await _context.TransactionFiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TransactionFileId,
            cancellationToken);

        if (transactionFile == null)
            throw new NotFoundException("Transaction file not found.");

        try
        {
            transactionFile.StartProcessing(_timeProvider.GetUtcNow().UtcDateTime);

            await _context.SaveChangesAsync(cancellationToken);

            var importedrows = await _mediator.Send(
                new ImportTransactionsCommand(request.TransactionFileId), cancellationToken);

            transactionFile.Complete(_timeProvider.GetUtcNow().UtcDateTime);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch(Exception ex)
        {
            transactionFile.Fail(
                _timeProvider.GetUtcNow().UtcDateTime, ex.Message);
            await _context.SaveChangesAsync(cancellationToken);

            throw;
        }
    }
}
