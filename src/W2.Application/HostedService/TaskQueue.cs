using Elsa.Activities.Signaling.Models;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using W2.HostedService;

public class TaskQueue : ITaskQueue
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2); // Limit to 10 concurrent tasks
    private readonly Channel<Func<CancellationToken, Task>> _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>();

    public async Task EnqueueAsync(Func<CancellationToken, Task> task)
    {
        await _queue.Writer.WriteAsync(task);
    }

    public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken); // Wait for a free slot
        return await _queue.Reader.ReadAsync(cancellationToken);
    }

    public void TaskCompleted()
    {
        _semaphore.Release(); // Release a slot
    }
}
