using FinGuard.Application.Commons.Interfaces;
using FinGuard.Infrastructure.Jobs;
using Hangfire;

namespace FinGuard.Infrastructure.Hangfire;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;

    public HangfireBackgroundJobService(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient;
    }

    public void EnqueueProcessTransactionFile(Guid transactionFileId)
    {
        _jobClient.Enqueue<ProcessTransactionFileJob>(job => 
        job.Execute(transactionFileId, CancellationToken.None));
    }
}
