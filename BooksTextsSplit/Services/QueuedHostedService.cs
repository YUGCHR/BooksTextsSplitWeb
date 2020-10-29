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
        //private readonly ILogger<QueuedHostedService> _logger;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue
            /*ILogger<QueuedHostedService> logger*/)
        {
            TaskQueue = taskQueue;
            //_logger = logger;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine(
                $"Queued Hosted Service is running.{Environment.NewLine}" +
                $"{Environment.NewLine}Tap W to add a work item to the " +
                $"background queue.{Environment.NewLine}");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            Console.WriteLine("Start");

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("Loop");
                var workItem =
                    await TaskQueue.DequeueAsync(stoppingToken);
                Console.WriteLine("Dequeued a task");
                try
                {
                    Console.WriteLine("Start await task");
                    await workItem(stoppingToken);
                    Console.WriteLine("Finished a work item");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        "Error occurred executing {0} {1}", nameof(workItem), ex.Message);
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
