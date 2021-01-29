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
        //private readonly IBackgroundTasksService _task2Queue;
        //private readonly ICacheProviderAsync _cache;
        //private readonly IPubSubProvider _pubSub;
        //private readonly IKeyEventsProvider _events;
        private readonly ILogger<MonitorLoop> _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly IOnKeysEventsSubscribeService _subscribe;

        public MonitorLoop(
            //IBackgroundTaskQueue taskQueue,
            ILogger<MonitorLoop> logger,
            IHostApplicationLifetime applicationLifetime,
            //IBackgroundTasksService task2Queue,
            //ICacheProviderAsync cache,
            //IPubSubProvider pubSub,
            //IKeyEventsProvider events,
            IOnKeysEventsSubscribeService subscribe)
        {
            //_taskQueue = taskQueue;
            _logger = logger;
            //_task2Queue = task2Queue;
            //_cache = cache;
            //_events = events;
            _subscribe = subscribe;
            //_pubSub = pubSub;
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
            // To start tasks batch enter from Redis console the command - hset subscribeOnFrom tasks:count 30 (where 30 is tasks count - from 10 to 50)

            KeyEvent eventCmdSet = KeyEvent.HashSet; 
            // все ключи положить в константы
            string eventKeyFrom = "subscribeOnFrom"; // ключ для подписки на команду запуска эмулятора сервера
            string eventFieldFrom = "count";
            string eventKeyRun = "task:run"; // ключ и поле для подписки на ключи задач, создаваемые сервером (или эмулятором)
            string eventFieldRun = "ttt"; 
            
            _subscribe.SubscribeOnEventFrom(eventKeyFrom, eventFieldFrom, eventCmdSet, eventKeyRun, eventFieldRun);

            _subscribe.SubscribeOnEventRun(eventKeyRun, eventCmdSet, eventKeyRun, eventFieldRun);

            //string eventKeyAdd = "task:add";
            //_subscribe.SubscribeOnEventAdd(eventKeyAdd, eventCmdSet);

            //KeyEvent eventCmdDel = KeyEvent.Delete;

            //string eventKey = "subscribeOnFrom";
            
            //_subscribe.SubscribeOnEventFrom(eventKey, eventCmdDel);

            bool cancellationIsNotYet = !_cancellationToken.IsCancellationRequested; // add special key from Redis?

            while (cancellationIsNotYet)
            {
                var keyStroke = Console.ReadKey();

                if (keyStroke.Key == ConsoleKey.W)
                {
                    _logger.LogInformation("ConsoleKey was received {KeyStroke}.", keyStroke.Key);
                }
            }
        }
    }
}
