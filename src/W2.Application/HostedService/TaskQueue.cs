using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace W2.HostedService
{
    public class TaskQueue : ITaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _tasks = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void EnqueueAsync(Func<CancellationToken, Task> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _tasks.Enqueue(task);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _tasks.TryDequeue(out var task);
            return task;
        }
    }
}
