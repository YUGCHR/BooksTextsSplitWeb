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
            //_keyEvents.Subscribe(KeyEventSubscriptionType.All, (key, cmd) =>
            //{
            //    _logger.LogInformation("key {0} - command {1}", key, cmd);
            //});

            string eventKey = "task:run";

            Action<string, KeyEvent> keyCmd = KeySub;
            _keyEvents.Subscribe(eventKey, keyCmd);

            string eventKeyCommand = $"Key {eventKey}, HashSet command";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);

            bool cancellationNotYet = !_cancellationToken.IsCancellationRequested; // add special key from Redis

            while (cancellationNotYet)
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
                _logger.LogInformation("key {Key} - command {Cmd} received, task sent to Queue ", key, cmd); 
                _task2Queue.StartWorkItem();
            }
        }

    }
}
