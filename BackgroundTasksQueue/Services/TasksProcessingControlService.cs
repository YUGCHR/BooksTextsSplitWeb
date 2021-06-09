using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Logging;
using Shared.Library.Models;
using Shared.Library.Services;

namespace BackgroundTasksQueue.Services
{
    public interface ITasksProcessingControlService
    {
        public Task<(bool, int)> CheckingPackageCompletion(ConstantsSet constantsSet, string tasksPackageGuidField);
        public Task<bool> CheckingAllTasksCompletion(ConstantsSet constantsSet, string tasksPackageGuidField);
    }

    public class TasksProcessingControlService : ITasksProcessingControlService
    {
        private readonly IBackgroundTasksService _task2Queue;
        private readonly ILogger<TasksProcessingControlService> _logger;
        private readonly ICacheManageService _cache;

        public TasksProcessingControlService(
            ILogger<TasksProcessingControlService> logger,
            ICacheManageService cache,
            IBackgroundTasksService task2Queue)
        {
            _task2Queue = task2Queue;
            _logger = logger;
            _cache = cache;
        }

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<TasksProcessingControlService>();
        // Verbose
        public async Task<(bool, int)> CheckingPackageCompletion(ConstantsSet constantsSet, string tasksPackageGuidField)
        {
            // проверить значение в ключе сервера - если больше нуля, значит, ещё не закончено
            // если пакет в работе, вернуть true, если пакет закончен - false
            string backServerPrefixGuid = constantsSet.BackServerPrefixGuid.Value;
            int totalUnsolvedTasksLeft = await _cache.FetchHashedAsync<int>(backServerPrefixGuid, tasksPackageGuidField); // forsake

            return (totalUnsolvedTasksLeft > 0, totalUnsolvedTasksLeft);
        }

        public async Task<bool> CheckingAllTasksCompletion(ConstantsSet constantsSet, string tasksPackageGuidField)
        {
            // проверяем текущее состояние пакета задач, если ещё выполняется, возобновляем подписку на ключ пакета
            // если выполнение окончено, подписку возобновляем или нет? но тогда восстанавливаем ключ подписки на вброс пакетов задач
            // возвращаем состояние выполнения - ещё выполняется или уже окончено
            // если выполняется, то true

            // достаём из каждого поля ключа значение (проценты) и вычисляем общий процент выполнения
            double taskPackageState = 0;
            bool allTasksCompleted = true;
            IDictionary<string, TaskDescriptionAndProgress> taskPackage = await _cache.FetchHashedAllAsync<TaskDescriptionAndProgress>(tasksPackageGuidField);
            int taskPackageCount = taskPackage.Count;
            Logs.Here().Debug("TasksList fetched - {@C}.", new { Count = taskPackageCount });

            foreach (var t in taskPackage)
            {
                var (singleTaskGuid, taskProgressState) = t;
                int taskState = taskProgressState.TaskState.TaskCompletedOnPercent;
                bool isTaskRunning = taskProgressState.TaskState.IsTaskRunning;
                if (isTaskRunning)
                {
                    allTasksCompleted = false; // если хоть одна задача выполняется, пакет не закончен
                }
                if (taskState < 0)
                {
                    // подсчёт всех процентов можно убрать, ориентироваться только на allTasksCompleted
                    // суммарный процент можно считать в другом методе или из этого возвращать принудительно сотню, если true
                    Logs.Here().Debug("One (or more) Tasks do not start yet - {@S}.", new { State = taskState });

                    return false;
                }
                // вычислить суммарный процент - всё сложить и разделить на количество
                taskPackageState += taskState;
                Logs.Here().Debug("foreach in taskPackage - Single Task completed on {1} percents. \n {@T} \n", taskState, new { Task = singleTaskGuid });
            }

            double taskPackageStatePercentageDouble = taskPackageState / taskPackageCount;
            int taskPackageStatePercentage = (int)taskPackageStatePercentageDouble;
            Logs.Here().Debug("RETURN - this TaskPackage is completed on {0} percents.", taskPackageStatePercentage);

            // подписку оформить в отдельном методе, а этот вызывать оттуда
            // можно ставить блокировку на подписку и не отвлекаться на события, пока не закончена очередная проверка

            return allTasksCompleted;
        }
    }
}
