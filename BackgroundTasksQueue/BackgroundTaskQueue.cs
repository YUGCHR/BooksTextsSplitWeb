using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackgroundTasksQueue.Models;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Logging;
using Shared.Library.Models;
using Shared.Library.Services;

namespace BackgroundTasksQueue
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);

        public int AddCarrierProcesses(ConstantsSet constantsSet, CancellationToken stoppingToken, int requiredProcessesCountToAdd);

        public int CancelCarrierProcesses(ConstantsSet constantsSet, CancellationToken stoppingToken, int requiredProcessesCountToAdd);

        public int CarrierProcessesCount(ConstantsSet constantsSet, int requiredProcessesCountToAdd);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ILogger<BackgroundTaskQueue> _logger;
        private readonly ICacheManageService _cache;
        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        List<BackgroundProcessingTask> _existingCarrierProcesses = new List<BackgroundProcessingTask>();

        public BackgroundTaskQueue(
            ILogger<BackgroundTaskQueue> logger,
            ICacheManageService cache)
        {
            _logger = logger;
            _cache = cache;
        }

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<BackgroundTaskQueue>();

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            // Adds an object to the end of the System.Collections.Concurrent.ConcurrentQueue`1
            _workItems.Enqueue(workItem);
            Logs.Here().Verbose("Single Task placed in Concurrent Queue.");

            _signal.Release();
            Logs.Here().Verbose("Concurrent Queue is ready for new task.");

        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            Logs.Here().Verbose("Single Task was dequeued from Queue.");
            return workItem;
        }

        // тут нужны 3 метода - создание процессов, проверка количества процессов, удаление процессов
        // вся регулировка должна быть внутри класса, внешний метод сообщает только необходимое ему количество процессов
        // всё же процессы это поля в ключе и сделать полностью изолированными - желательно вынести в отдельный класс - нет
        // или можно сообщать количество задач в пакете, а класс сам решит, сколько надо/можно пакетов - перенести метод подсчёта сюда
        // отдельные методы добавления и убавления, подписки не надо
        // главный метод, который считает и решает, ему приходит вызов от подписки с ключа сервера про задачу
        // надо ли ждать, когда все задачи загрузятся? особого смысла нет
        // можно добавить переключатель автоматически/вручную и ручную (+/-) регулировку количества процессов в настройках веб-интерфейса

        public int AddCarrierProcesses(ConstantsSet constantsSet, CancellationToken stoppingToken, int requiredProcessesCountToAdd)
        {
            // можно поставить блокировку одновременного вызова методов добавления/удаления

            // здесь requiredProcessesCountToAdd заведомо больше нуля
            int addedProcessesCount = 0;
            int totalProcessesCount = _existingCarrierProcesses.Count;
            Logs.Here().Debug("CarrierProcesses addition is started, required additional count = {0}, total count was {1}.", requiredProcessesCountToAdd, totalProcessesCount);

            // поменять while на for
            while (addedProcessesCount < requiredProcessesCountToAdd && !stoppingToken.IsCancellationRequested)
            {
                string guid = Guid.NewGuid().ToString();
                CancellationTokenSource newCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                CancellationToken newToken = newCts.Token;
                Logs.Here().Debug("CarrierProcessesManager creates process No {0}.", totalProcessesCount);

                // создаём новый процесс, к гуид можно добавить какой-то префикс
                BackgroundProcessingTask newlyAddedProcess = new BackgroundProcessingTask()
                {
                    // можно поставить глобальный счётчик процессов в классе - лучше счётчик в выделенном поле этого же ключа
                    TaskId = totalProcessesCount,
                    ProcessingTaskId = guid,
                    // запускаем новый процесс
                    ProcessingTask = Task.Run(() => ProcessingTaskMethod(newToken), newToken),
                    CancellationTaskToken = newCts
                };

                _existingCarrierProcesses.Add(newlyAddedProcess);

                // новое значение общего количества процессов и номер для следующего
                totalProcessesCount++;
                // счётчик добавленных процессов для while
                addedProcessesCount++;
                Logs.Here().Debug("New Task for Background Processes was added, total count became {0}.", totalProcessesCount);
            }

            int checkedProcessesCount = _existingCarrierProcesses.Count;
            Logs.Here().Debug("New processes count was checked, count++ = {0}, total count = {1}.", totalProcessesCount, checkedProcessesCount);

            return checkedProcessesCount;
        }

        public int CancelCarrierProcesses(ConstantsSet constantsSet, CancellationToken stoppingToken, int requiredProcessesCountToCancel)
        {
            int totalProcessesCount = _existingCarrierProcesses.Count;
            Logs.Here().Debug("CarrierProcesses removing is started, necessary excess count = {0}, total count was {1}.", requiredProcessesCountToCancel, totalProcessesCount);

            // всё же надо проверить, что удаляем меньше, чем существует
            if (requiredProcessesCountToCancel < totalProcessesCount)
            {
                int existedProcessesCount = totalProcessesCount;
                Logs.Here().Debug("_existingCarrierProcesses.Count with existing processes labels was fetched, count = {0}.", existedProcessesCount);

                // не foreach и не while, а for - раз заранее известно точное количество проходов
                for (int i = 0; i < requiredProcessesCountToCancel; i++)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return 0;
                    }
                    CancellationTokenSource cts = _existingCarrierProcesses[i].CancellationTaskToken;
                    // сливаем процесс
                    cts.Cancel();

                    _existingCarrierProcesses.RemoveAt(i);
                    totalProcessesCount--;
                    Logs.Here().Debug("Process liable to removing was deleted, Cycle {0} from {1}, processes left {2}.", i, requiredProcessesCountToCancel, totalProcessesCount);
                }
                Logs.Here().Debug("Some Background Processes was deleted, \n total count was {0}, deletion request was {1}, now count became {0}.", existedProcessesCount, requiredProcessesCountToCancel, totalProcessesCount);
                int leftProcessesCount = _existingCarrierProcesses.Count; 
                Logs.Here().Debug("Actual Processes labels count is {0}.", leftProcessesCount);

                return leftProcessesCount;
            }

            return 0;
        }

        public int CarrierProcessesCount(ConstantsSet constantsSet, int requiredProcessesCountToAdd)
        {
            int totalProcessesCount = _existingCarrierProcesses.Count;
            Logs.Here().Debug("CarrierProcesses total count is {0}.", totalProcessesCount);
            
            return totalProcessesCount;
        }

        private async Task ProcessingTaskMethod(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var workItem = await DequeueAsync(token); // was TaskQueue

                try
                {
                    await workItem(token);
                }
                catch (Exception ex)
                {
                    Logs.Here().Error("Error occurred executing {0}.", ex);
                }
            }
        }

    }
}

