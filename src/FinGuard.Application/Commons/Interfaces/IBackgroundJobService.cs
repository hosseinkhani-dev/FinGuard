namespace FinGuard.Application.Commons.Interfaces;

public interface IBackgroundJobService
{
    void EnqueueProcessTransactionFile(Guid transactionFileId);
}
