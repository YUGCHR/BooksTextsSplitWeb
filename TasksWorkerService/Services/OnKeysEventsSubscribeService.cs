using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackgroundTasksQueue.Models;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Logging;

namespace BackgroundTasksQueue.Services
{
    public interface IOnKeysEventsSubscribeService
    {
        void SubscribeOnEventFrom(string eventKey, string eventFieldFrom, KeyEvent eventCmd, string eventKeyRun, string eventFieldRun);
        void SubscribeOnEventRun(string eventKey, KeyEvent eventCmd, string eventKeyRun, string eventFieldRun);
        void SubscribeOnEventAdd(string eventKey, KeyEvent eventCmd);
    }

    public class OnKeysEventsSubscribeService : IOnKeysEventsSubscribeService
    {
        private readonly IBackgroundTasksService _task2Queue;
        //private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<OnKeysEventsSubscribeService> _logger;
        private readonly ICacheProviderAsync _cache;
        //private readonly IPubSubProvider _pubSub;
        private readonly IKeyEventsProvider _keyEvents;
        private readonly IFrontServerEmulationService _front;

        public OnKeysEventsSubscribeService(
            //IBackgroundTaskQueue taskQueue,
            ILogger<OnKeysEventsSubscribeService> logger, 
            ICacheProviderAsync cache, 
            //IPubSubProvider pubSub, 
            IKeyEventsProvider keyEvents, 
            IFrontServerEmulationService front, 
            IBackgroundTasksService task2Queue)
        {
            _task2Queue = task2Queue;
            //_taskQueue = taskQueue;
            _logger = logger;
            _cache = cache;
            //_pubSub = pubSub;
            _keyEvents = keyEvents;
            _front = front;

        }

        public void SubscribeOnEventFrom(string eventKey, string eventFieldFrom, KeyEvent eventCmd, string eventKeyRun, string eventFieldRun)
        {
            //string eventKeyFrom = "subscribeOnFrom";
            //KeyEvent eventCmd = KeyEvent.HashSet;

            _keyEvents.Subscribe(eventKey, (string key, KeyEvent cmd) =>
            {
                if (cmd == eventCmd)
                {
                    _logger.LogInformation("Key {Key} with command {Cmd} was received.", eventKey, cmd);
                    _front.FrontServerEmulation(eventKey, eventFieldFrom, eventKeyRun, eventFieldRun).Wait();
                }
            });

            string eventKeyCommand = $"Key = {eventKey}, Command = {eventCmd}";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);
        }

        public void SubscribeOnEventRun(string eventKey, KeyEvent eventCmd, string eventKeyRun, string eventFieldRun)
        {
            _keyEvents.Subscribe(eventKey, (string key, KeyEvent cmd) =>
            {
                if (cmd == eventCmd)
                {
                    _logger.LogInformation("Key {Key} with command {Cmd} was received.", eventKey, cmd);

                    FetchKeysOnEventRun(eventKeyRun, eventFieldRun).Wait();
                }
            });

            string eventKeyCommand = $"Key = {eventKey}, Command = {eventCmd}";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);
        }

        private async Task FetchKeysOnEventRun(string eventKeyRun, string eventFieldRun)
        {
            _logger.LogInformation("Task FetchKeysOnEventRun with key {0} and field {1} started.", eventKeyRun, eventFieldRun);

            string taskPackageKey = await _cache.GetHashedAsync<string>(eventKeyRun, eventFieldRun); // получили ключ, где находится пакет заданий

            _logger.LogInformation("taskPackageKey {0} was received.", taskPackageKey);

            IDictionary<string, int> taskPackage = await _cache.GetHashedAllAsync<int>(taskPackageKey); // получили пакет заданий - id задачи и данные (int) для неё

            foreach (var t in taskPackage) // try to Select?
            {
                var (guid, cycleCount) = t;
                _task2Queue.StartWorkItem(guid, cycleCount);
                _logger.LogInformation("Task with ID {0} and {1} cycles was sent to Queue.", guid, cycleCount);
            }
        }

        public void SubscribeOnEventAdd(string eventKey, KeyEvent eventCmd)
        {
            string eventKeyCommand = $"Key {eventKey}, HashSet command";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);
        }

    }
}
