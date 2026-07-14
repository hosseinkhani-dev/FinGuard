using FinGuard.Application.Commons.Interfaces;
using FinGuard.Domain.Entities;
using MediatR;

namespace FinGuard.Application.Features.TransactionFiles.Commands.CreateTransactionFile;

public class CreateTransactionFileCommandHandler :
    IRequestHandler<CreateTransactionFileCommand, Guid>
{
    private readonly IFinGuardDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IBackgroundJobService _backgroundJobService;

    public CreateTransactionFileCommandHandler(
        IFinGuardDbContext context,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IBackgroundJobService backgroundJobService)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _backgroundJobService = backgroundJobService;
    }


    public async Task<Guid> Handle(CreateTransactionFileCommand request, CancellationToken cancellationToken)
    {
        var transactionFile = new TransactionFile(
            _currentUser.UserId,
            request.OriginalFileName,
            request.StoredFileName,
            request.storagePath,
            request.FileSize,
            _timeProvider.GetUtcNow().UtcDateTime);

        _context.TransactionFiles.Add(transactionFile);
        await _context.SaveChangesAsync(cancellationToken);

        _backgroundJobService.EnqueueProcessTransactionFile(transactionFile.Id);

        return transactionFile.Id;
    }
}
