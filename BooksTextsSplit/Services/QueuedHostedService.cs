using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BooksTextsSplit.Services
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger)
        {
            TaskQueue = taskQueue;
            _logger = logger;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"Queued Hosted Service is running with concurrent 3 Tasks.{Environment.NewLine}" +
                $"{Environment.NewLine}Send get Worker to add a work item to the " +
                $"background queue.{Environment.NewLine}");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            //Console.WriteLine("Start");
            Action<CancellationToken> process = async token =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    //Console.WriteLine("Loop");
                    var workItem =
                        await TaskQueue.DequeueAsync(stoppingToken);
                    //Console.WriteLine("Dequeued a task");
                    try
                    {
                        //Console.WriteLine("Start await task");
                        await workItem(stoppingToken);
                        //Console.WriteLine("Finished a work item");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error occurred executing {WorkItem}.", nameof(workItem));
                    }
                }
            };

            var task1 = Task.Run(() => process(stoppingToken), stoppingToken);
            var task2 = Task.Run(() => process(stoppingToken), stoppingToken);
            var task3 = Task.Run(() => process(stoppingToken), stoppingToken);

            await Task.WhenAll(task1, task2, task3);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
