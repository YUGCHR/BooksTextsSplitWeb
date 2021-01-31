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
    public interface IFrontServerEmulationService
    {
        public Task FrontServerEmulation(string eventKeyFrom, string eventFieldFrom, string eventKeyRun, string eventFieldRun); // все ключи положить в константы

    }

    public class FrontServerEmulationService : IFrontServerEmulationService
    {
        //private readonly IBackgroundTasksService _task2Queue;
        //private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<FrontServerEmulationService> _logger;
        private readonly ICacheProviderAsync _cache;
        //private readonly IPubSubProvider _pubSub;
        //private readonly IKeyEventsProvider _keyEvents;
        //private readonly IOnKeysEventsSubscribeService _eventsSubscribe;

        public FrontServerEmulationService(
            //IBackgroundTaskQueue taskQueue,
            ILogger<FrontServerEmulationService> logger,
            ICacheProviderAsync cache
            //IPubSubProvider pubSub, 
            //IKeyEventsProvider keyEvents, 
            //IOnKeysEventsSubscribeService eventsSubscribe, 
            //IBackgroundTasksService task2Queue
            )
        {
            //_task2Queue = task2Queue;
            //_taskQueue = taskQueue;
            _logger = logger;
            _cache = cache;
            //_pubSub = pubSub;
            //_keyEvents = keyEvents;
            //_eventsSubscribe = eventsSubscribe;

        }

        public async Task FrontServerEmulation(string eventKeyFrom, string eventFieldFrom, string eventKeyRun, string eventFieldRun)
        {
            // эмулятор сервера создаёт задачу, даёт ей айди, кладёт в специальный ключ и потом другим ключом сообщает, что есть задача
            // задачи класть по одной или сразу списком? наверное, задачи (главы) одной книги можно класть списком
            string packageGuid = Guid.NewGuid().ToString(); // создаём имя ключа, содержащего пакет задач

            int tasksCount = await _cache.GetHashedAsync<int>(eventKeyFrom, eventFieldFrom); //получить число задач

            _logger.LogInformation("TaskCount = {TasksCount} from key {Key} was fetched.", tasksCount, eventKeyFrom);

            if (tasksCount < 10) tasksCount = 10;
            if (tasksCount > 50) tasksCount = 50;

            Dictionary<string, int> taskPackage = new Dictionary<string, int>();

            for (int i = 0; i < tasksCount; i++)
            {
                string guid = Guid.NewGuid().ToString();
                int cycleCount = Math.Abs(guid.GetHashCode()) % 10;

                if (cycleCount < 3)
                {
                    cycleCount += 3;
                }

                taskPackage.Add(guid, cycleCount);
                _logger.LogInformation("Task {I} from {TasksCount} with ID {Guid} and {CycleCount} cycles was added to taskPackage key.", i, tasksCount, guid, cycleCount);
            }

            //IEnumerable<Task> e = taskPackage.Select(async t =>
            //    {
            //        var (guid, cycleCount) = t;
            //        await _cache.SetHashedAsync(packageGuid, guid, cycleCount); // записываем пакет ключей с данными для каждой задачи или можно записать один ключ с пакетом (лист) задач
            //        _logger.LogInformation("Key {0}, field {1} with {2} cycles was set.", packageGuid, guid, cycleCount);
            //    });

            foreach (KeyValuePair<string, int> t in taskPackage)
            {
                (string guid, int cycleCount) = t;
                await _cache.SetHashedAsync(packageGuid, guid, cycleCount); // записываем пакет ключей с данными для каждой задачи или можно записать один ключ с пакетом (лист) задач
                _logger.LogInformation("Key {0}, field {1} with {2} cycles was set.", packageGuid, guid, cycleCount);
            }

            await _cache.SetHashedAsync(eventKeyRun, eventFieldRun, packageGuid); // создаём ключ, на который подписана очередь и в значении передаём имя ключа, содержащего пакет задач
            _logger.LogInformation("Key {0}, field {1} with {2} KeyName was set.", eventKeyRun, eventFieldRun, packageGuid);
        }

        //public async Task FrontServerEmulation(string eventKeyFrom)
        //{
        //    int tasksCount = await _cache.GetHashedAsync<int>(eventKeyFrom, "tasks:count"); //получить число задач
        //    _logger.LogInformation("TaskCount = {TasksCount} from key {Key}", tasksCount, eventKeyFrom);
        //    if (tasksCount < 10) tasksCount = 10;
        //    if (tasksCount > 50) tasksCount = 50;
        //    for (int i = 0; i < tasksCount; i++)
        //    {
        //        string guid = Guid.NewGuid().ToString();
        //        int cycleCount = Math.Abs(guid.GetHashCode()) % 10;
        //        _task2Queue.StartWorkItem(guid, cycleCount);
        //        _logger.LogInformation("Task {I} from {TasksCount} with ID {Guid} and {CycleCount} cycles sent to Queue ", i, tasksCount, guid, cycleCount);
        //    }
        //}
    }
}
