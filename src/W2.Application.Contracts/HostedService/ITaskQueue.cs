using System;
using System.Threading.Tasks;
using System.Threading;

namespace W2.HostedService
{
    public interface ITaskQueue
    {
        void EnqueueAsync(Func<CancellationToken, Task> task);
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
