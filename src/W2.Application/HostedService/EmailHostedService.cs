﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace W2.HostedService
{
    public class EmailHostedService : BackgroundService
    {
        private readonly ITaskQueue _taskQueue;
        private readonly ILogger<EmailHostedService> _logger;

        public EmailHostedService(ITaskQueue taskQueue,
            ILogger<EmailHostedService> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var emailTask = await _taskQueue.DequeueAsync(stoppingToken);
                _logger.LogError($"ERR {DateTime.Now} queue {_taskQueue.GetQueueCount()}");
                try
                {
                    await emailTask(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                finally
                {
                    _taskQueue.TaskCompleted();
                }
            }
        }
    }
}
