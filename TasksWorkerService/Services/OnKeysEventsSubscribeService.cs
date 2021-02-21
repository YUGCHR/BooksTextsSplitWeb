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
        public Task<string> FetchGuidFieldTaskRun(string eventKeyRun, string eventFieldRun);
        public void SubscribeOnEventFrom(string eventKey, string eventFieldFrom, KeyEvent eventCmd, string eventKeyRun, string eventFieldRun);
        public void SubscribeOnEventRun(string eventKey, KeyEvent eventCmd, string eventFieldRun, int processNum);
        public void SubscribeOnEventAdd(string eventKey, KeyEvent eventCmd);
    }

    public class OnKeysEventsSubscribeService : IOnKeysEventsSubscribeService
    {
        private readonly IBackgroundTasksService _task2Queue;
        private readonly ILogger<OnKeysEventsSubscribeService> _logger;
        private readonly ICacheProviderAsync _cache;
        private readonly IKeyEventsProvider _keyEvents;
        private readonly IFrontServerEmulationService _front;

        public OnKeysEventsSubscribeService(
            ILogger<OnKeysEventsSubscribeService> logger,
            ICacheProviderAsync cache,
            IKeyEventsProvider keyEvents,
            IFrontServerEmulationService front,
            IBackgroundTasksService task2Queue)
        {
            _task2Queue = task2Queue;
            _logger = logger;
            _cache = cache;
            _keyEvents = keyEvents;
            _front = front;
        }

        public async Task<string> FetchGuidFieldTaskRun(string eventKeyRun, string eventFieldRun)
        {
            await _front.FrontServerEmulationCreateGuidField(eventKeyRun, eventFieldRun); // создаём эмулятором сервера guid поле для ключа "task:run" (и сразу же его читаем)

            string eventGuidFieldRun = await _cache.GetHashedAsync<string>(eventKeyRun, eventFieldRun); //получить guid поле для "task:run"

            return eventGuidFieldRun;
        }

        public void SubscribeOnEventFrom(string eventKey, string eventFieldFrom, KeyEvent eventCmd, string eventKeyRun, string eventGuidFieldRun)
        {
            //string eventKeyFrom = "subscribeOnFrom";
            //KeyEvent eventCmd = KeyEvent.HashSet;

            _keyEvents.Subscribe(eventKey, async (string key, KeyEvent cmd) =>
            {
                if (cmd == eventCmd)
                {
                    _logger.LogInformation("Key {Key} with command {Cmd} was received.", eventKey, cmd);
                    await _front.FrontServerEmulationMain(eventKey, eventFieldFrom, eventKeyRun, eventGuidFieldRun);
                }
            });

            string eventKeyCommand = $"Key = {eventKey}, Command = {eventCmd}";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);
        }

        public void SubscribeOnEventRun(string eventKey, KeyEvent eventCmd, string eventGuidFieldRun, int processNum)
        {
            _logger.LogInformation("Background server No: {EventKey} started.", processNum);

            _keyEvents.Subscribe(eventKey, async (string key, KeyEvent cmd) =>
            {
                if (cmd == eventCmd)
                {
                    _logger.LogInformation("Key {Key} with command {Cmd} was received.", eventKey, cmd);
                    await FetchKeysOnEventRun(eventKey, eventGuidFieldRun, processNum);
                }
            });

            string eventKeyCommand = $"Key = {eventKey}, Command = {eventCmd}";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);
        }

        private async Task FetchKeysOnEventRun(string eventKeyRun, string eventGuidFieldRun, int processNum)
        {
            _logger.LogInformation("Task FetchKeysOnEventRun with key {0} and field {1} started.", eventKeyRun, eventGuidFieldRun);

            // добавить проверку, что получаемое значение - правильный guid (try/catch)
            string taskPackageKey = await _cache.GetHashedAsync<string>(eventKeyRun, eventGuidFieldRun); // получили ключ, где находится пакет заданий

            // проверяем захват задачи - пробуем удалить поле ключа
            bool isDeleteSuccess = await _cache.RemoveHashedAsync(eventKeyRun, eventGuidFieldRun);
            _logger.LogInformation("Background server No: {0} reported - isDeleteSuceess = {1}.", processNum, isDeleteSuccess);

            // когда номер сервера будет guid, очистку можно убрать, но поставить срок жизни ключа час или день, при нормальном завершении пакета ключ удаляется штатно
            bool isKeyCleanedSuccess = await _cache.RemoveAsync(processNum.ToString()); 
            _logger.LogInformation("Background server No: {0} reported - Server Key Cleaned Success = {1}.", processNum, isKeyCleanedSuccess);

            if (isDeleteSuccess)
            {
                _logger.LogInformation("Background server No: {0} fetched taskPackageKey {1}.", processNum, taskPackageKey); // победитель по жизни

                IDictionary<string, int> taskPackage = await _cache.GetHashedAllAsync<int>(taskPackageKey); // получили пакет заданий - id задачи и данные (int) для неё

                foreach (var t in taskPackage) // try to Select?
                {
                    var (guid, cycleCount) = t;
                    _task2Queue.StartWorkItem(processNum.ToString(), guid, cycleCount);
                    await _cache.SetHashedAsync(processNum.ToString(), guid, cycleCount); // создаём ключ для контроля выполнения задания из пакета
                    _logger.LogInformation("Background server No: {0} sent Task with ID {1} and {2} cycles to Queue.", processNum, guid, cycleCount);
                }
                return;
            }
            _logger.LogInformation("Background server No: {0} cannot catch the task.", processNum);
            return;
        }

        public void SubscribeOnEventAdd(string eventKey, KeyEvent eventCmd)
        {
            string eventKeyCommand = $"Key {eventKey}, HashSet command";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);
        }

    }
}
