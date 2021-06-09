using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Logging;
using Shared.Library.Models;
using Shared.Library.Services;

namespace BackgroundTasksQueue.Services
{
    public interface ITasksPackageCaptureService
    {
        public Task<string> AttemptToCaptureTasksPackage(ConstantsSet constantsSet, CancellationToken stoppingToken);
    }

    public class TasksPackageCaptureService : ITasksPackageCaptureService
    {
        private readonly IBackgroundTasksService _task2Queue;
        private readonly ILogger<TasksPackageCaptureService> _logger;
        private readonly ICacheManageService _cache;

        public TasksPackageCaptureService(
            ILogger<TasksPackageCaptureService> logger,
            ICacheManageService cache,
            IBackgroundTasksService task2Queue)
        {
            _task2Queue = task2Queue;
            _logger = logger;
            _cache = cache;
        }

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<TasksPackageCaptureService>();

        public async Task<string> AttemptToCaptureTasksPackage(ConstantsSet constantsSet, CancellationToken stoppingToken) // Main for Capture
        {
            string backServerPrefixGuid = constantsSet.BackServerPrefixGuid.Value;
            string eventKeyFrontGivesTask = constantsSet.EventKeyFrontGivesTask.Value;
            string eventKeyBacksTasksProceed = constantsSet.EventKeyBacksTasksProceed.Value;
            Logs.Here().Debug("BackServer started AttemptToCaptureTasksPackage.");

            // начало главного цикла сразу после срабатывания подписки, условие - пока существует ключ распределения задач
            // считать пакет полей из ключа, если задач больше одной, бросить кубик
            // проверить захват задачи, если получилось - выполнять, нет - вернулись на начало главного цикла
            // выполнение - в отдельном методе, достать по ключу задачи весь пакет
            // определить, сколько надо процессов - количество задач в пакете разделить на константу, не менее одного и не более константы
            // запустить процессы в отдельном методе, сложить количество в ключ пакета
            // достать задачи из пакета и запустить их в очередь
            // следующим методом висеть и контролировать ход выполнения всех задач - подписаться на их ключи, собирать ход выполнения каждой, суммировать и складывать общий процент в ключ сервера
            // по окончанию всех задач удалить все процессы?
            // вернуться на начало главного цикла
            bool isExistEventKeyFrontGivesTask = true;

            // нет смысла проверять isDeleteSuccess, достаточно существования ключа задач - есть он, ловим задачи, нет его - возвращаемся
            while (isExistEventKeyFrontGivesTask) // может и надо поставить while всегда - все условия выхода внутри и по ним будет р
            {
                // проверить существование ключа, может, все задачи давно разобрали и ключ исчез
                Logs.Here().Debug("KeyExistsAsync will call now.");
                isExistEventKeyFrontGivesTask = await _cache.IsKeyExist(eventKeyFrontGivesTask);
                Logs.Here().Debug("KeyFrontGivesTask {@E}.", new { isExisted = isExistEventKeyFrontGivesTask });

                if (!isExistEventKeyFrontGivesTask)
                    // если ключа нет, тогда возвращаемся в состояние подписки на ключ кафе и ожидания события по этой подписке
                {
                    Logs.Here().Debug("Main way - return to the subscription on the cafe key.");
                    return null;
                } // задача не досталась

                // после сообщения подписки об обновлении ключа, достаём список свободных задач
                // список получается неполный! - оказывается, потому, что фронт не успеваем залить остальные поля, когда бэк с первым полем уже здесь
                IDictionary<string, string> tasksList = await _cache.FetchHashedAllAsync<string>(eventKeyFrontGivesTask);
                int tasksListCount = tasksList.Count;
                Logs.Here().Debug("TasksList fetched - {@T}.", new { TaskCount = tasksListCount });

                // временный костыль - 0 - это задач в ключе не осталось - возможно, только что (перед носом) забрали последнюю
                if (tasksListCount == 0)
                    // тогда возвращаемся в состояние подписки на ключ кафе и ожидания события по этой подписке
                    // поскольку тогда следить за выполнением не надо, возвращаем null - и где-то его заменят на true
                {
                    Logs.Here().Warning("Spare Way - return to the subscription on the cafe key.");
                    return null; // задача не досталась
                } 

                // выбираем случайное поле пакета задач - скорее всего, первая попытка будет только с одним полем, остальные не успеют положить и будет драка, но на второй попытке уже разойдутся по разным полям
                (string tasksPackageGuidField, string tasksPackageGuidValue) = tasksList.ElementAt(DiceRoll(constantsSet, tasksListCount));

                // проверяем захват задачи - пробуем удалить выбранное поле ключа                
                // в дальнейшем можно вместо Remove использовать RedLock
                bool isDeleteSuccess = await _cache.DelFieldAsync(eventKeyFrontGivesTask, tasksPackageGuidField);
                Logs.Here().Debug("BackServer reported - {@D}.", new { TasksPackageFieldWasDeletedSuccessfully = isDeleteSuccess });

                if (isDeleteSuccess)
                {
                    // тут перейти в TasksBatchProcessingService
                    // не перейти, а вернуться в подписку с номером пакета задач
                    Logs.Here().Debug("Task Package fetched. \n {@G}", new { Package = tasksPackageGuidField });
                    // проверяли, что и вправду удалили поле, а то были сомнения
                    //IDictionary<string, string> tasksList1 = await _cache.GetHashedAllAsync<string>(eventKeyFrontGivesTask);
                    //int tasksListCount1 = tasksList1.Count;
                    //Logs.Here().Debug("TasksList after removing fetched - {@T}.", new { TaskCountNew = tasksListCount1 });
                    return tasksPackageGuidField;
                }
            }

            // пока что сюда никак попасть не может, но надо предусмотреть, что все задачи исчерпались, а никого не поймали
            // скажем, ключ вообще исчез и ловить больше нечего
            // теперь сюда попадём, если ключ eventKeyFrontGivesTask исчез и задачу не захватить
            // надо сделать возврат в исходное состояние ожидания вброса ключа
            // побочный эффект - можно смело брать последнюю задачу и не опасаться, что ключ eventKeyFrontGivesTask исчезнет
            // возвращаемся в состояние подписки на ключ кафе и ожидания события по этой подписке
            // восстанавливаем условие разрешения обработки подписки
            return null; // задача не досталась
        }

        private static int DiceRoll(ConstantsSet constantsSet, int tasksListCount)
        {
            // базовое значение кубика - если вдруг одна задача
            int diceRoll = (tasksListCount - 1);
            Logs.Here().Debug("DiceRoll is prepared to roll {@F}.", new { BaseValue = diceRoll });

            // если осталась одна задача, кубик бросать не надо
            if (tasksListCount > 1)
            {
                int diceRollRow = RandomProvider.Next(0, constantsSet.RandomRangeExtended.Value);
                Logs.Here().Debug("DiceRollRow rolled {@F}.", new { RowValue = diceRollRow });
                diceRoll = diceRollRow % (tasksListCount - 1);
            }
            Logs.Here().Debug("DiceRoll rolled {@F}.", new { Facet = diceRoll });
            
            return diceRoll;
        }
    }
}
