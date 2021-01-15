using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CachingFramework.Redis.Contracts.Providers;
using BackgroundTasksQueue.Models;

namespace BackgroundTasksQueue.Services
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;
        private readonly ICacheProviderAsync _cache;
        private readonly IKeyEventsProvider _keyEvents;

        List<BackgroundProcessingTask> tasks = new List<BackgroundProcessingTask>();


        public QueuedHostedService(IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger, ICacheProviderAsync cache, IKeyEventsProvider keyEvents)
        {
            TaskQueue = taskQueue;
            _logger = logger;
            _cache = cache;
            _keyEvents = keyEvents;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Queued Hosted Service is running.{Environment.NewLine}" +
                                   $"{Environment.NewLine}Tap W to add a work item to the " +
                                   $"background queue.{Environment.NewLine}");
            _logger.LogInformation("Queued Hosted Service waits W.");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            //Action<CancellationToken> process = async token =>
            //{
            //    while (!stoppingToken.IsCancellationRequested)
            //    {
            //        var workItem = await TaskQueue.DequeueAsync(stoppingToken);

            //        try
            //        {
            //            await workItem(stoppingToken);
            //        }
            //        catch (Exception ex)
            //        {
            //            _logger.LogError(ex, "Error occurred executing {WorkItem}.", nameof(workItem));
            //        }
            //    }
            //};

            int tasksCount = 0;

            await _cache.SetObjectAsync("tasksCount", 3);

            //List<Task> tasks = new List<Task>();

            //for (int i = 0; i < tasksCount; i++)
            //{
            //    tasks.Add(Task.Run(() => ProcessingMethod(stoppingToken), stoppingToken));
            //}

            //Task task1 = ;
            //Task task2 = Task.Run(() => ProcessingMethod(stoppingToken), stoppingToken);
            //Task task3 = Task.Run(() => ProcessingMethod(stoppingToken), stoppingToken);

            //await Task.WhenAll(task1, task2, task3);

            string eventKey = "task:add";
            string cancelKey = "task:del";

            _keyEvents.Subscribe(eventKey, (string key, KeyEvent cmd) =>
            {
                if (cmd == KeyEvent.HashSet)
                {
                    _logger.LogInformation("key {0} - command {1}", key, cmd);

                    string guid = Guid.NewGuid().ToString();
                    CancellationTokenSource newCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    CancellationToken newToken = newCts.Token;

                    // положить newCts в словарь (айди таска, цтс)
                    tasks.Add(new BackgroundProcessingTask()
                    {
                        TaskId = tasksCount + 1,
                        ProcessingTaskId = guid,
                        ProcessingTask = Task.Run(() => ProcessingMethod(newToken), newToken),
                        CancellationTaskToken = newCts
                    });
                    tasksCount++;

                    //tasks.Add(Task.Run(() => ProcessingMethod(newToken), newToken));
                    _logger.LogInformation("New Task for Background Processes was added, total count became {Count}", tasksCount);
                }
                Console.WriteLine("command " + cmd);
            });

            string eventKeyCommand = $"Key {eventKey}, HashSet command";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);

            _keyEvents.Subscribe(cancelKey, (string key, KeyEvent cmd) =>
            {
                if (cmd == KeyEvent.HashSet)
                {
                    _logger.LogInformation("key {0} - command {1}", key, cmd);
                    if (tasksCount > 0)
                    {
                        var cts = tasks[tasksCount - 1].CancellationTaskToken;
                        cts.Cancel();

                        tasks.RemoveAt(tasksCount - 1);
                        tasksCount--;
                        _logger.LogInformation("One Task for Background Processes was removed, total count left {Count}", tasksCount);
                    }
                    else
                    {
                        _logger.LogInformation("Task for Background Processes cannot be removed for some reason, total count is {Count}", tasksCount);
                    }
                    //tasks.Add(Task.Run(() => ProcessingMethod(newToken), newToken));
                }
                Console.WriteLine("command " + cmd);
            });

            List<Task> processingTask = tasks.Select(t => t.ProcessingTask).ToList();
            
            await Task.WhenAll(processingTask);

            _logger.LogInformation("All Background Processes were finished, total count was {Count}", processingTask.Count);
        }

        private async Task ProcessingMethod(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(token);

                try
                {
                    await workItem(token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");


            await base.StopAsync(stoppingToken);
        }

        public void StopTask(int taskId)
        {
            //var cts = CtsDictionary[taskId];
            //cts.Cancel();
        }
    }
}
