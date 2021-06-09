using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shared.Library.Models;
using Shared.Library.Services;

namespace BackgroundTasksQueue.Services
{
    public interface ITasksBatchProcessingService
    {
        public Task WhenTasksPackageWasCaptured(ConstantsSet constantsSet, string tasksPackageGuidField, CancellationToken stoppingToken);
    }

    public class TasksBatchProcessingService : ITasksBatchProcessingService
    {
        private readonly IBackgroundTasksService _task2Queue;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ICacheManageService _cache;

        public TasksBatchProcessingService(
            ICacheManageService cache,
            IBackgroundTasksService task2Queue, 
            IBackgroundTaskQueue taskQueue)
        {
            _task2Queue = task2Queue;
            _taskQueue = taskQueue;
            _cache = cache;
        }

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<TasksBatchProcessingService>();

        public async Task WhenTasksPackageWasCaptured(ConstantsSet constantsSet, string tasksPackageGuidField, CancellationToken stoppingToken) // Main for Processing
        {
            string backServerPrefixGuid = constantsSet.BackServerPrefixGuid.Value;
            Logs.Here().Debug("This BackServer fetched Task Package successfully. \n {@P}", new { Package = tasksPackageGuidField });

            // регистрируем полученный пакет задач на ключе выполняемых/выполненных задач и на ключе сервера
            // ключ выполняемых задач надо переделать - в значении класть модель, в которой указан номер сервера и состояние задачи
            // скажем, List of TaskDescriptionAndProgress и в нём дополнительное поле номера сервера и состояния всего пакета

            // перенесли RegisterTasksPackageGuid после TasksFromKeysToQueue, чтобы получить taskPackageCount для регистрации 

            // тут подписаться (SubscribeOnEventCheck) на ключ пакета задач для контроля выполнения, но будет много событий
            // каждая задача будет записывать в этот ключ своё состояние каждый цикл - надо ли так делать?

            // и по завершению выполнения задач хорошо бы удалить процессы
            // нужен внутрисерверный ключ (константа), где каждый из сервисов (каждого) сервера может узнать номер сервера, на котором запущен - чтобы правильно подписаться на событие
            // сервера одинаковые и жёлуди у них тоже одинаковые, разница только в номере, который сервер генерирует при своём старте
            // вот этот номер нужен сервисам, чтобы подписаться на события своего сервера, а не соседнего                    

            // складываем задачи во внутреннюю очередь сервера
            // tasksPakageGuidValue больше не нужно передавать, вместо нее tasksPackageGuidField

            Logs.Here().Verbose("RegisterTasksPackageGuid called.");
            await RegisterTasksPackageGuid(constantsSet, tasksPackageGuidField, stoppingToken);
            Logs.Here().Verbose("RegisterTasksPackageGuid finished.");

            // здесь ждать команды от резолвера процессов о готовности
            // или удалять все процессы после пакета
            // или удалять до минимума и всегда только добавлять

            // не надо сообщать резолверу ключ пакета через подписку -
            // надо его просто вызвать, дать ему ключ и ждать ответа от него -
            // тогда процессы уже будут готовы

            int actualProcessesCountAfterCorrection = await CarrierProcessesSolver(tasksPackageGuidField, constantsSet, stoppingToken);

            Logs.Here().Verbose("TasksFromKeysToQueue called.");
            int taskPackageCount = await TasksFromKeysToQueue(constantsSet, tasksPackageGuidField, stoppingToken);
            Logs.Here().Verbose("TasksFromKeysToQueue finished with Task Count = {0}.", taskPackageCount);

            // здесь подходящее место, чтобы определить количество процессов, выполняющих задачи из пакета - в зависимости от количества задач, но не более максимума из константы
            // PrefixProcessAdd - префикс ключа (+ backServerGuid) управления добавлением процессов
            // PrefixProcessCancel - префикс ключа (+ backServerGuid) управления удалением процессов
            // в значение положить требуемое количество процессов
            // имя поля должно быть общим для считывания значения
            // PrefixProcessCount - 
            // не забыть обнулить (или удалить) ключ после считывания и добавления процессов - можно и не удалять, всё равно, пока его не перепишут, он больше никого не интересует
            // можно в качестве поля использовать гуид пакета задач, но, наверное, это лишние сложности, всё равно процессы общие
            
            // тут ждать, пока не будут посчитаны всё задачи пакета
            // теперь не будем ждать, вернём false и пусть ждут другую подписку, которая поменяет на true
            //int completionPercentage = 1; // await CheckingAllTasksCompletion(tasksPackageGuidField);
            
            // тут удалить все процессы (потом)
            // процессы тоже не здесь удаляем - перенести их отсюда
            //int cancelExistingProcesses = await CancelExistingProcesses(eventKeysSet, addProcessesCount, completionPercentage);
            // выйти из цикла можем только когда не останется задач в ключе кафе
        }

        // решатель подписан на ключ сервера и узнаёт о появлении нового пакета задач при его регистрации
        // после сработки подписки надо её сразу (заблокировать?) отпустить, оставив себе только гуид пакета
        // сходить в пакет и узнать, сколько там задач, исходя из этого, решить, сколько на пакет нужно процессов
        // проверить, сколько процессов существует сейчас, посчитать, как изменить и отдать команду менеджеру с количеством
        // проверить результат, вернуть актуальное количество процессов после корректировки
        private async Task<int> CarrierProcessesSolver(string tasksPackageGuidField, ConstantsSet constantsSet, CancellationToken stoppingToken)
        {
            // зная номер пакета, можно сразу получить количество задач в нём с ключа сервера
            string backServerPrefixGuid = constantsSet.BackServerPrefixGuid.Value;
            int taskPackageCount = await _cache.FetchHashedAsync<int>(backServerPrefixGuid, tasksPackageGuidField);
            Logs.Here().Debug("taskPackageCount was fetched, Task Count = {0}.", taskPackageCount);

            // вычисляем нужное количество процессов для такого количества задач
            int neededProcessesCount = CalcNeededProcessesCountForPackage(constantsSet, taskPackageCount);
            Logs.Here().Debug("neededProcessesCount was calculated, Processes needed = {0}.", neededProcessesCount);

            // узнаем, сколько существует процессов сейчас (0 - только получить количество существующих процессов, без изменения)
            int actualProcessesCount = await CarrierProcessesManager(constantsSet, stoppingToken, 0);
            Logs.Here().Debug("actualProcessesCount was fetched, Processes exist = {0}.", actualProcessesCount);

            // или делать это в CarrierProcessesManager, подумать

            // узнаем, как надо изменить число процессов - увеличить/уменьшить/оставить
            int correctionProcessesCount = neededProcessesCount - actualProcessesCount;
            Logs.Here().Debug("correctionProcessesCount was calculated, correction needed = {0}.", correctionProcessesCount);

            // корректируем количество процессов
            int correctedProcessesCount = await CarrierProcessesManager(constantsSet, stoppingToken, correctionProcessesCount);

            // сравнить correctedProcessesCount с actualProcessesCountAfterCorrection - должны быть одинаковые
            int actualProcessesCountAfterCorrection = await CarrierProcessesManager(constantsSet, stoppingToken, 0);
            Logs.Here().Information("actualProcessesCountAfterCorrection was created, actual processes = {0}.", actualProcessesCountAfterCorrection);

            Logs.Here().Debug("Process request was resolved, cacl. count = {0}, checked count = {1}.", correctedProcessesCount, actualProcessesCountAfterCorrection);

            // если неодинаковые, вывести ошибку

            // здесь создать ключ, который даст команду грузить задачи в очередь - уже не надо

            return actualProcessesCountAfterCorrection;
        }

        // менеджер получает требуемое количество добавить/убавить/сообщить
        // решением, сколько нужно всего процессов, занимаются выше
        // возвращает результат действия - количество добавленных, убранных или посчитанных
        private async Task<int> CarrierProcessesManager(ConstantsSet constantsSet, CancellationToken stoppingToken, int requiredProcessesCount)
        {
            //int actualProcessesCount = 0;
            switch (requiredProcessesCount)
            {
                case < 0:
                    Logs.Here().Debug("required processes count < 0, Count = {0}, needs to call CancelCarrierProcesses.", requiredProcessesCount);
                    // удалять просто так нельзя, надо сделать это до выгрузки задач в очередь
                    // сначала надо сообщить, что процессы готовы и тогда начнут загружать задачи
                    int requiredProcessesCountToCancel = Math.Abs(requiredProcessesCount);
                    int canceledProcessesCount = _taskQueue.CancelCarrierProcesses(constantsSet, stoppingToken, requiredProcessesCountToCancel);
                    return canceledProcessesCount;
                case 0:
                    Logs.Here().Debug("required processes count = 0, Count = {0}, needs to call CarrierProcessesCount.", requiredProcessesCount);
                    // можно только сообщить количество процессов, без изменения количества
                    int actualProcessesCount = _taskQueue.CarrierProcessesCount(constantsSet, 0);
                    return actualProcessesCount;
                case > 0:
                    Logs.Here().Debug("required processes count > 0, Count = {0}, needs to call AddCarrierProcesses.", requiredProcessesCount);
                    // возвращает актуальное расчетное - не проверенное подсчётом полей - количество процессов
                    int addedProcessesCount = _taskQueue.AddCarrierProcesses(constantsSet, stoppingToken, requiredProcessesCount);
                    return addedProcessesCount;
            }
        }

        private int CalcNeededProcessesCountForPackage(ConstantsSet constantsSet, int taskPackageCount)
        {
            int balanceOfTasksAndProcesses = constantsSet.BalanceOfTasksAndProcesses.Value;
            int maxProcessesCountOnServer = constantsSet.MaxProcessesCountOnServer.Value;
            int toAddProcessesCount;

            switch (balanceOfTasksAndProcesses)
            {
                // 0 - автовыбор - создаём процессов по числу задач
                case 0:
                    toAddProcessesCount = taskPackageCount;
                    Logs.Here().Debug("CalcNeededProcessesCountForPackage - balance = {0}, Task Count = {1}, needed processes count = {2}.", balanceOfTasksAndProcesses, taskPackageCount, toAddProcessesCount);

                    return toAddProcessesCount;
                // больше нуля - основной вариант - делим количество задач на эту константу и если она больше максимума, берём константу максимума
                case > 0:
                    int multiplier = 10000; // from constants
                    toAddProcessesCount = (taskPackageCount * multiplier / balanceOfTasksAndProcesses) / multiplier;
                    // если константа максимума неправильная - 0 или отрицательная, игнорируем ее
                    if (toAddProcessesCount > maxProcessesCountOnServer && maxProcessesCountOnServer > 0)
                    {
                        toAddProcessesCount = maxProcessesCountOnServer;
                    }
                    if (toAddProcessesCount < 1)
                    { toAddProcessesCount = 1; }
                    Logs.Here().Debug("CalcNeededProcessesCountForPackage - balance = {0}, Task Count = {1}, needed processes count = {2}.", balanceOfTasksAndProcesses, taskPackageCount, toAddProcessesCount);

                    return toAddProcessesCount;
                // меньше нуля - тайный вариант для настройки - количество процессов равно константе (с обратным знаком, естественно)
                case < 0:
                    toAddProcessesCount = Math.Abs(balanceOfTasksAndProcesses);
                    Logs.Here().Debug("CalcNeededProcessesCountForPackage - balance = {0}, Task Count = {1}, needed processes count = {2}.", balanceOfTasksAndProcesses, taskPackageCount, toAddProcessesCount);

                    return toAddProcessesCount;
            }
        }

        private async Task<bool> RegisterTasksPackageGuid(ConstantsSet constantsSet, string tasksPackageGuidField, CancellationToken stoppingToken)
        {
            IDictionary<string, TaskDescriptionAndProgress> taskPackage = await _cache.FetchHashedAllAsync<TaskDescriptionAndProgress>(tasksPackageGuidField); // получили пакет заданий - id задачи и данные (int) для неё
            int taskPackageCount = taskPackage.Count;

            string backServerPrefixGuid = constantsSet.BackServerPrefixGuid.Value;
            //string eventKeyFrontGivesTask = eventKeysSet.EventKeyFrontGivesTask;
            string eventKeyBacksTasksProceed = constantsSet.EventKeyBacksTasksProceed.Value;
            
            // следующие две регистрации пока непонятно, зачем нужны - доступ к состоянию пакета задач всё равно по ключу пакета
            // очень нужен - для контроля окончания выполнения задачи и пакета

            // регистрируем полученную задачу на ключе выполняемых/выполненных задач
            // поле - исходный ключ пакета (известный контроллеру, по нему он найдёт сервер, выполняющий его задание)
            // пока что поле задачи в кафе и ключ самой задачи совпадают, поэтому контроллер может напрямую читать состояние пакета задач по известному ему ключу
            // ключ выполняемых задач надо переделать - в значении класть модель, в которой указан номер сервера и состояние задачи
            // скажем, List of TaskDescriptionAndProgress и в нём дополнительное поле номера сервера и состояния всего пакета

            await _cache.WriteHashedAsync(eventKeyBacksTasksProceed, tasksPackageGuidField, backServerPrefixGuid, constantsSet.PrefixBackServer.LifeTime);
            Logs.Here().Debug("Tasks package was registered. \n {@P} \n {@E}", new{Package = tasksPackageGuidField }, new{ EventKey = eventKeyBacksTasksProceed });

            // регистрируем исходный ключ и ключ пакета задач на ключе сервера - чтобы не разорвать цепочку
            // цепочка уже не актуальна, можно этот ключ использовать для контроля состояния пакета задач
            // для этого в дальнейшем в значение можно класть общее состояние всех задач пакета в процентах
            // или не потом, а сейчас класть 0 - тип значения менять нельзя
            // сейчас в значение кладём количество задач в пакете, а про мере выполнения вычитаем по единичке, чтобы как ноль - пакет выполнен
            int packageStateInit = taskPackageCount;
            await _cache.WriteHashedAsync(backServerPrefixGuid, tasksPackageGuidField, packageStateInit, constantsSet.PrefixBackServer.LifeTime);
            Logs.Here().Verbose("This BackServer registered task package and RegisterTasksPackageGuid returned true.");

            // создаём ключ add для сообщения решателю процессов о новом пакете и сообщаем ключ пакета
            // дальше он сам справился, не маленький
            // не создаём
            // в смысле, уже не нужно
            //string eventKeyProcessAdd = eventKeysSet.ProcessAddPrefixGuid;
            //string eventFieldBack = eventKeysSet.EventFieldBack;
            //await _cache.SetHashedAsync(eventKeyProcessAdd, eventFieldBack, tasksPackageGuidField); // TimeSpan.FromDays - !!!
            //Logs.Here().Information("Key {0} about new package was created for processes solver. \n {@F} \n {@P}", eventKeyProcessAdd, new { Field = eventFieldBack }, new { Package = tasksPackageGuidField });

            return true;
        }
        
        private async Task<int> TasksFromKeysToQueue(ConstantsSet constantsSet, string tasksPackageGuidField, CancellationToken stoppingToken)
        {
            IDictionary<string, TaskDescriptionAndProgress> taskPackage = await _cache.FetchHashedAllAsync<TaskDescriptionAndProgress>(tasksPackageGuidField); // получили пакет заданий - id задачи и данные (int) для неё
            int taskPackageCount = taskPackage.Count;
            int sequentialSingleTaskNumber = 0;
            foreach (var t in taskPackage)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return 0;
                }
                var (singleTaskGuid, taskDescription) = t;

                // регистрируем задачи на ключе контроля выполнения пакета (prefixControlTasksPackageGuid)
                string prefixControlTasksPackageGuid = $"{constantsSet.PrefixPackageControl.Value}:{tasksPackageGuidField}";
                double tasksPackageTtl = constantsSet.PrefixPackageControl.LifeTime; // or constantsSet.PrefixBackServer.LifeTime
                await _cache.WriteHashedAsync<int>(prefixControlTasksPackageGuid, singleTaskGuid, sequentialSingleTaskNumber, constantsSet.PrefixBackServer.LifeTime);
                Logs.Here().Debug("Single task {0} was registered on Completed Control Key. \n {@P} \n {@S}", sequentialSingleTaskNumber, new { PackageControl = prefixControlTasksPackageGuid }, new { SingleTask = singleTaskGuid });
                sequentialSingleTaskNumber++;

                // складываем задачи во внутреннюю очередь сервера
                _task2Queue.StartWorkItem(constantsSet, tasksPackageGuidField, tasksPackageTtl, singleTaskGuid, taskDescription, stoppingToken);
                // создаём ключ для контроля выполнения задания из пакета - нет, создаём не тут и не такой (ключ)
                //await _cache.SetHashedAsync(backServerPrefixGuid, singleTaskGuid, assignmentTerms); 
                Logs.Here().Verbose("This BackServer sent Task to Queue. \n {@T}", new { Task = singleTaskGuid }, new { CyclesCount = taskDescription.TaskDescription.CycleCount });
            }
            Logs.Here().Verbose("This BackServer sent total {0} tasks to Queue.", taskPackageCount);
            return taskPackageCount;
        }
    }
}
