using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Logging;

namespace OldBackgroundTasksQueue.Services
{
    public interface IFrontServerEmulationService
    {
        public Task FrontServerEmulationCreateGuidField(string eventKeyRun, string eventFieldRun);
        public Task FrontServerEmulationMain(string eventKeyFrom, string eventFieldFrom, string eventKeyRun, string eventFieldRun); // все ключи положить в константы

    }

    public class FrontServerEmulationService : IFrontServerEmulationService
    {
        private readonly ILogger<FrontServerEmulationService> _logger;
        private readonly ICacheProviderAsync _cache;

        public FrontServerEmulationService(ILogger<FrontServerEmulationService> logger, ICacheProviderAsync cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public async Task FrontServerEmulationCreateGuidField(string eventKeyRun, string eventFieldRun)
        {
            string eventGuidFieldRun = Guid.NewGuid().ToString(); // 

            await _cache.SetHashedAsync(eventKeyRun, eventFieldRun, eventGuidFieldRun); // создаём ключ ("task:run"), на который подписана очередь и в значении передаём имя ключа, содержащего пакет задач

            _logger.LogInformation("Guid Field {0} for key {1} was created and set.", eventGuidFieldRun, eventKeyRun);
        }

        public async Task FrontServerEmulationMain(string eventKeyFrom, string eventFieldFrom, string eventKeyRun, string eventFieldRun)
        {
            // эмулятор сервера создаёт задачу, даёт ей айди, кладёт в специальный ключ и потом другим ключом сообщает, что есть задача
            // задачи класть по одной или сразу списком? наверное, задачи (главы) одной книги можно класть списком

            string packageGuid = Guid.NewGuid().ToString(); // создаём имя ключа, содержащего пакет задач

            int tasksCount = await FrontServerFetchConditions(eventKeyFrom, eventFieldFrom); // получаем условия задач по стартовому ключу

            Dictionary<string, int> taskPackage = FrontServerCreateTasks(tasksCount); // создаём пакет задач

            await FrontServerSetTasks(taskPackage, packageGuid); // записываем пакет задач в ключ packageGuid

            await _cache.SetHashedAsync(eventKeyRun, eventFieldRun, packageGuid); // создаём ключ ("task:run"), на который подписана очередь и в значении передаём имя ключа, содержащего пакет задач

            _logger.LogInformation("Key {0}, field {1} with {2} KeyName was set.", eventKeyRun, eventFieldRun, packageGuid);
        }

        private async Task<int> FrontServerFetchConditions(string eventKeyFrom, string eventFieldFrom)
        {
            int tasksCount = await _cache.GetHashedAsync<int>(eventKeyFrom, eventFieldFrom); //получить число задач (по этому ключу метод вызвали)

            _logger.LogInformation("TaskCount = {TasksCount} from key {Key} was fetched.", tasksCount, eventKeyFrom);

            if (tasksCount < 3) tasksCount = 3;
            if (tasksCount > 50) tasksCount = 50;

            return tasksCount;
        }

        private async Task FrontServerSetTasks(Dictionary<string, int> taskPackage, string packageGuid)
        {
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
        }

        private Dictionary<string, int> FrontServerCreateTasks(int tasksCount)
        {
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
            return taskPackage;
        }

        // здесь нужен метод, следящий за процентами выполнения заданий и, если какое-то остановилось, надо перезапустить
        // только перезапускать надо не здесь

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
