using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackgroundTasksQueue.Services;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundTasksQueue
{
    public class MonitorLoop
    {
        //private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IBackgroundTasksService _task2Queue;
        private readonly ICacheProviderAsync _cache;
        private readonly IPubSubProvider _pubSub;
        private readonly IKeyEventsProvider _keyEvents;
        private readonly ILogger<MonitorLoop> _logger;
        private readonly CancellationToken _cancellationToken;

        public MonitorLoop(
            //IBackgroundTaskQueue taskQueue,
            ILogger<MonitorLoop> logger,
            IHostApplicationLifetime applicationLifetime,
            IBackgroundTasksService task2Queue,
            ICacheProviderAsync cache,
            IPubSubProvider pubSub,
            IKeyEventsProvider keyEvents)
        {
            //_taskQueue = taskQueue;
            _logger = logger;
            _task2Queue = task2Queue;
            _cache = cache;
            _keyEvents = keyEvents;
            _pubSub = pubSub;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public void StartMonitorLoop()
        {
            _logger.LogInformation("Monitor Loop is starting.");

            // Run a console user input loop in a background thread
            Task.Run(Monitor, _cancellationToken);
        }

        public void Monitor()
        {
            string eventKeyFrom = "taskFrom";
            KeyEvent eventKeyHash = KeyEvent.HashSet;
            //Action<string, KeyEvent> keyCmd = CallFrontService;
            SubscribeKeyEventHashSet(eventKeyFrom, eventKeyHash);

            //string eventKey = "task:run";
            //Action<string, KeyEvent> keyCmd = KeySub;
            //_keyEvents.Subscribe(eventKey, keyCmd);
            //string eventKeyCommand = $"Key {eventKey}, HashSet command";
            //_logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);

            bool cancellationIsNotYet = !_cancellationToken.IsCancellationRequested; // add special key from Redis

            while (cancellationIsNotYet)
            {
                //_pubSub.Publish(eventKey, ("while"));

                var keyStroke = Console.ReadKey();

                if (keyStroke.Key == ConsoleKey.W)
                {
                    _logger.LogInformation("ConsoleKey was received {KeyStroke}.", keyStroke.Key);
                    //_pubSub.Publish(eventKey, ("if (keyStroke.Key == ConsoleKey.W)"));
                }
            }
        }

        private void KeySub(string key, KeyEvent cmd)
        {
            if (cmd == KeyEvent.HashSet)
            {
                string guid = Guid.NewGuid().ToString();
                _logger.LogInformation("key {Key} - command {Cmd} received, task {Guid} sent to Queue ", key, cmd, guid);
                //_task2Queue.StartWorkItem(guid);
            }
        }
        private void SubscribeKeyEventHashSet(string eventKey, KeyEvent eventKeyHash)
        {
            _keyEvents.Subscribe(eventKey, (string key, KeyEvent cmd) =>
            {
                if (cmd == eventKeyHash)
                {
                    _logger.LogInformation("Key {Key} with command {Cmd} was received.", eventKey, cmd);
                    ServerFrontEndEmulation(eventKey).Wait();
                }
            });
            string eventKeyCommand = $"Key - {eventKey}, Command - {eventKeyHash}";
            _logger.LogInformation("You subscribed on event - {Event}.", eventKeyCommand);
        }

        private void CallFrontService(string eventKeyFrom, KeyEvent cmd)
        {
            KeyEvent eventKeyHash = KeyEvent.HashSet;
            string eventKeyRun = "task:run";
            string eventFieldFromBase = "task:";

            if (cmd == eventKeyHash)
            {

            }
        }

        public async Task ServerFrontEndEmulation(string eventKeyFrom)
        {
            int tasksCount = await _cache.GetHashedAsync<int>(eventKeyFrom, "tasks:count"); //получить число задач
            _logger.LogInformation("TaskCount = {TasksCount} from key {Key}", tasksCount, eventKeyFrom);
            if (tasksCount < 10) tasksCount = 10;
            if (tasksCount > 50) tasksCount = 50;
            for (int i = 0; i < tasksCount; i++)
            {
                string guid = Guid.NewGuid().ToString();
                int cycleCount = Math.Abs(guid.GetHashCode()) % 10;
                _task2Queue.StartWorkItem(guid, cycleCount);
                _logger.LogInformation("Task {I} from {TasksCount} with ID {Guid} and {CycleCount} cycles sent to Queue ", i, tasksCount, guid, cycleCount);
            }
        }
    }
}
