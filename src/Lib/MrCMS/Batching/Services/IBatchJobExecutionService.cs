using System.Threading;
using System.Threading.Tasks;
using MrCMS.Batching.Entities;

namespace MrCMS.Batching.Services
{
    public interface IBatchJobExecutionService
    {
        Task<BatchJobExecutionResult> Execute(BatchJob batchJob, CancellationToken token);
    }
}