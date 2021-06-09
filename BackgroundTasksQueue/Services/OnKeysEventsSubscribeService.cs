using System.Threading;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using Shared.Library.Models;
using Shared.Library.Services;

namespace BackgroundTasksQueue.Services
{
    public interface IOnKeysEventsSubscribeService
    {
        public Task SubscribeOnEventRun(CancellationToken stoppingToken);
    }

    public class OnKeysEventsSubscribeService : IOnKeysEventsSubscribeService
    {
        private readonly ISettingConstants _constants;
        private readonly ICacheManageService _cache;
        private readonly IKeyEventsProvider _keyEvents;
        private readonly ITasksPackageCaptureService _captures;
        private readonly ITasksBatchProcessingService _processing;

        public OnKeysEventsSubscribeService(
            ISettingConstants constants,
            ICacheManageService cache,
            IKeyEventsProvider keyEvents,
            ITasksPackageCaptureService captures,
            ITasksBatchProcessingService processing)
        {
            _constants = constants;
            _cache = cache;
            _keyEvents = keyEvents;
            _captures = captures;
            _processing = processing;
        }

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<OnKeysEventsSubscribeService>();

        private bool _flagToBlockEventRun;
        private bool _flagToBlockEventCompleted;
        private int _callingNumOfCheckKeyFrontGivesTask; // for debug only

        // подписываемся на ключ сообщения о появлении свободных задач
        public async Task SubscribeOnEventRun(CancellationToken stoppingToken)
        {
            // подписаться в самом начале кода и при смене guid дата-сервера будет заново выполняться подписка на этот guid
            // к сожалению эту подписку пришлось перенести в SharedDataAccess, потому что она там изменяет глобальный флаг класса
            _constants.SubscribeOnBaseConstantEvent();

            // все проверки и ожидание внутри вызываемого метода, без констант не вернётся
            ConstantsSet constantsSet = await _constants.ConstantInitializer(stoppingToken);
            // но можно проверять на null, если как-то null, то что-то сделать (shutdown?)
            //EventKeyNames eventKeysSet = await _constants.ConstantInitializer(stoppingToken); // for memory

            string eventKeyFrontGivesTask = constantsSet.EventKeyFrontGivesTask.Value;
            Logs.Here().Debug("BackServer subscribed on EventKey. \n {@E}", new { EventKey = eventKeyFrontGivesTask });
            Logs.Here().Information("Constants version is {0}:{1}.", constantsSet.ConstantsVersionBase.Value, constantsSet.ConstantsVersionNumber.Value);

            // блокировка множественной подписки до специального разрешения повторной подписки
            _flagToBlockEventRun = true;
            // на старте вывecти состояние всех глобальных флагов
            //Logs.Here().Debug("SubscribeOnEventRun started with the following flags: => \n {@F1} \n {@F2} \n {@F3}", new { FlagToBlockEventRun = _flagToBlockEventRun }, new { EventCompletedTaskWasHappening = _eventCompletedTaskWasHappening }, new { ProcessingEventCompletedTaskIsLaunched = _processingEventCompletedTaskIsLaunched });

            _keyEvents.Subscribe(eventKeyFrontGivesTask, (string key, KeyEvent cmd) =>
            {
                // скажем, в подписке вызывается метод проверить наличие пакетов в кафе(CheckKeyFrontGivesTask), если пакеты есть, он возвращает true, и метод за ним (FreshTaskPackageHasAppeared) начинает захват пакета
                if (cmd == constantsSet.EventCmd && _flagToBlockEventRun)
                {
                    // подписка заблокирована
                    _flagToBlockEventRun = false;
                    // быструю блокировку оставить - когда ещё отпишемся, но можно сделать локальной?
                    Logs.Here().Debug("CheckKeyFrontGivesTask will be called No:{0}, Event permit = {Flag} \n {@K} with {@C} was received. \n", _callingNumOfCheckKeyFrontGivesTask, _flagToBlockEventRun, new { Key = eventKeyFrontGivesTask }, new { Command = cmd });
                    
                    _ = CheckKeyFrontGivesTask(constantsSet, stoppingToken);
                }
            });

            string eventKeyCommand = $"Key = {eventKeyFrontGivesTask}, Command = {constantsSet.EventCmd}";
            Logs.Here().Debug("You subscribed on EventSet. \n {@ES}", new { EventSet = eventKeyCommand });
        }

        private async Task<bool> CheckKeyFrontGivesTask(ConstantsSet constantsSet, CancellationToken stoppingToken) // Main of EventKeyFrontGivesTask key
        {
            _callingNumOfCheckKeyFrontGivesTask++;
            if (_callingNumOfCheckKeyFrontGivesTask > 1)
            {
                Logs.Here().Warning("CheckKeyFrontGivesTask was called more than once - Calling Count = {0}.", _callingNumOfCheckKeyFrontGivesTask);
            }

            // тут определить, надо ли обновить константы
            bool isExistUpdatedConstants = _constants.IsExistUpdatedConstants();
            Logs.Here().Information("Is Exist Updated Constants = {0}.", isExistUpdatedConstants);

            if (isExistUpdatedConstants)
            {
                constantsSet = await _constants.ConstantInitializer(stoppingToken); //EventKeyNames
                Logs.Here().Information("Updated Constant = {0}.", constantsSet.TaskEmulatorDelayTimeInMilliseconds.Value);
            }

            string eventKeyFrontGivesTask = constantsSet.EventKeyFrontGivesTask.Value;
            // проверить существование ключа - если ключ есть, надо идти добывать пакет
            Logs.Here().Debug("KeyFrontGivesTask will be checked now.");
            bool isExistEventKeyFrontGivesTask = await _cache.IsKeyExist(eventKeyFrontGivesTask);
            Logs.Here().Debug("KeyFrontGivesTask {@E}.", new { isExisted = isExistEventKeyFrontGivesTask });

            if (isExistEventKeyFrontGivesTask)
            {
                // отменить подписку глубже, когда получится захватить пакет?
                _ = FreshTaskPackageHasAppeared(constantsSet, stoppingToken);
                Logs.Here().Debug("FreshTaskPackageHasAppeared was passed, Subscribe permit = {Flag}.", _flagToBlockEventRun);

                Logs.Here().Debug("CheckKeyFrontGivesTask finished No:{0}.", _callingNumOfCheckKeyFrontGivesTask);
                _callingNumOfCheckKeyFrontGivesTask--;

                return false;
            }

            // всё_протухло - пакетов нет, восстановить подписку (also Subscribe to Update) и ждать погоду
            _flagToBlockEventRun = true;

            Logs.Here().Information("This Server finished current work.\n {@S} \n Global {@PR} \n", new { Server = constantsSet.BackServerPrefixGuid.Value }, new { Permit = _flagToBlockEventRun });
            Logs.Here().Warning("Next package could not be obtained - there are no more packages in cafe.");
            string packageSeparator1 = new('Z', 80);
            Logs.Here().Warning("This Server waits new Task Package. \n {@S} \n {1} \n", new { Server = constantsSet.BackServerPrefixGuid.Value }, packageSeparator1);

            // для отладки контролировали количество одновременных вызовов, чтобы не было больше одного
            Logs.Here().Debug("CheckKeyFrontGivesTask finished No:{0}.", _callingNumOfCheckKeyFrontGivesTask);
            _callingNumOfCheckKeyFrontGivesTask--;

            return true;
        }

        private async Task<bool> FreshTaskPackageHasAppeared(ConstantsSet constantsSet, CancellationToken stoppingToken)
        {
            // вернуть все подписки сюда
            // метод состоит из трёх частей -
            // 1 попытка захвата пакета задач, если ни один пакет захватить не удалось, возвращаемся обратно в эту подписку ждать следующих пакетов
            // 2 если пакет захвачен, подписываемся на его гуид
            // 3 начинаем обработку - регистрация, помещение задач в очередь и создание нужного количества процессов
            // если всё удачно, возвращаемся сюда, оставив подписку заблокированной

            string tasksPackageGuidField = await _captures.AttemptToCaptureTasksPackage(constantsSet, stoppingToken);

            // если flagToBlockEventRun null, сразу возвращаемся с true для возобновления подписки
            if (tasksPackageGuidField != null)
            {
                Logs.Here().Debug("AttemptToCaptureTasksPackage captured the TaskPackage. \n {@T}.", new { TaskPackage = tasksPackageGuidField });
                string packageSeparator0 = new('#', 90);
                Logs.Here().Warning("AttemptToCaptureTasksPackage captured new TaskPackage. \n {0} \n", packageSeparator0);

                SubscribeOnEventPackageCompleted(constantsSet, tasksPackageGuidField, stoppingToken);
                _ = _processing.WhenTasksPackageWasCaptured(constantsSet, tasksPackageGuidField, stoppingToken);

                Logs.Here().Debug("WhenTasksPackageWasCaptured passed without awaiting.");
                // всегда возвращаем false - задачи отправлены в работу и подписку восстановит модуль контроля завершения пакета
                // и ещё сначала проверит, не остались ли ещё других пакетов в кафе
                return false;
            }
            // возвращаем true, потому что задачу добыть не удалось, пакетов больше нет и надо ждать следующего вброса
            _flagToBlockEventRun = true;

            Logs.Here().Information("This Server finished current work.\n {@S} \n Global {@PR} \n", new { Server = constantsSet.BackServerPrefixGuid.Value }, new { Permit = _flagToBlockEventRun });
            Logs.Here().Warning("Next package could not be obtained - there are no more packages in cafe.");
            string packageSeparator1 = new('-', 80);
            Logs.Here().Warning("This Server waits new Task Package. \n {@S} \n {1} \n", new { Server = constantsSet.BackServerPrefixGuid.Value }, packageSeparator1);

            return true;
        }

        private void SubscribeOnEventPackageCompleted(ConstantsSet constantsSet, string tasksPackageGuidField, CancellationToken stoppingToken)
        {
            // подписка на окончание единичной задачи (для проверки, все ли задачи закончились)
            _flagToBlockEventCompleted = true;
            string backServerPrefixGuid = constantsSet.BackServerPrefixGuid.Value;
            string prefixPackageCompleted = constantsSet.PrefixPackageCompleted.Value;
            string prefixCompletedTasksPackageGuid = $"{prefixPackageCompleted}:{tasksPackageGuidField}";
            Logs.Here().Information("BackServer subscribed on EventKey Server Guid. \n {@E}", new { EventKey = prefixCompletedTasksPackageGuid });

            _keyEvents.Subscribe(prefixCompletedTasksPackageGuid, (string key, KeyEvent cmd) => // async before action
            {
                if (cmd == constantsSet.EventCmd && _flagToBlockEventCompleted)
                {
                    _flagToBlockEventCompleted = false;

                    Logs.Here().Debug("SubscribeOnEventPackageCompleted was called with event ---current_package_finished---.");
                    _ = CheckKeyFrontGivesTask(constantsSet, stoppingToken);
                    Logs.Here().Debug("CheckKeyFrontGivesTask was called and passed.");
                }
            });

            string eventKeyCommand = $"Key = {prefixCompletedTasksPackageGuid}, Command = {constantsSet.EventCmd}";
            Logs.Here().Debug("You subscribed on EventSet. \n {@ES}", new { EventSet = eventKeyCommand });
        }
    }
}
