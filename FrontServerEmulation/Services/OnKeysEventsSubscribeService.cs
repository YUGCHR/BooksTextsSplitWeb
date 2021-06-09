using System;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Logging;
using Shared.Library.Models;
using Shared.Library.Services;

namespace FrontServerEmulation.Services
{
    public interface IOnKeysEventsSubscribeService
    {
        public Task<string> FetchGuidFieldTaskRun(string eventKeyRun, string eventFieldRun, double ttl);
        public void SubscribeOnEventFrom(ConstantsSet constantsSet);        
    }

    public class OnKeysEventsSubscribeService : IOnKeysEventsSubscribeService
    {
        
        private readonly ILogger<OnKeysEventsSubscribeService> _logger;
        private readonly ICacheManageService _cache;
        private readonly IKeyEventsProvider _keyEvents;
        private readonly IFrontServerEmulationService _front;

        public OnKeysEventsSubscribeService(
            ILogger<OnKeysEventsSubscribeService> logger,
            ICacheManageService cache,
            IKeyEventsProvider keyEvents,
            IFrontServerEmulationService front
            )
        {
            _logger = logger;
            _cache = cache;
            _keyEvents = keyEvents;
            _front = front;
        }

        public async Task<string> FetchGuidFieldTaskRun(string eventKeyRun, string eventFieldRun, double ttl) // not used
        {
            await _front.FrontServerEmulationCreateGuidField(eventKeyRun, eventFieldRun, ttl); // создаём эмулятором сервера guid поле для ключа "task:run" (и сразу же его читаем)

            string eventGuidFieldRun = await _cache.FetchHashedAsync<string>(eventKeyRun, eventFieldRun); //получить guid поле для "task:run"

            return eventGuidFieldRun;
        }

        public void SubscribeOnEventFrom(ConstantsSet constantsSet) // _logger = 201
        {
            // ждёт команды с консоли с количеством генерируемых пакетов            
            string eventKey = constantsSet.EventKeyFrom.Value;
            KeyEvent eventCmd = constantsSet.EventCmd;

            _keyEvents.Subscribe(eventKey, async (string key, KeyEvent cmd) =>
            {
                if (cmd == eventCmd)
                {
                    // по получению начинает цикл создания пакетов с задачами
                    _logger.LogInformation("Key {Key} with command {Cmd} was received.", eventKey, cmd);
                    int tasksPackagesCount = await _front.FrontServerEmulationMain(constantsSet);
                    _logger.LogInformation("Tasks Packages created in count = {0}.", tasksPackagesCount);

                }
            });

            string eventKeyCommand = $"Key = {eventKey}, Command = {eventCmd}";
            _logger.LogInformation("You subscribed on event - {EventKey}.", eventKeyCommand);
            _logger.LogInformation("To start the front emulation please send from Redis console the following command - \n{_}{0} {1} count NN (NN - packages count).", "      ", eventCmd, eventKey);
        }
    }
}
