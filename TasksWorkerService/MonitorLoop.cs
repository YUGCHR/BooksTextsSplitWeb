using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackgroundTasksQueue.Services;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundTasksQueue
{
    public class MonitorLoop
    {
        private readonly ILogger<MonitorLoop> _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly IOnKeysEventsSubscribeService _subscribe;

        public MonitorLoop(            
            ILogger<MonitorLoop> logger,
            IHostApplicationLifetime applicationLifetime,            
            IOnKeysEventsSubscribeService subscribe)
        {
            _logger = logger;
            _subscribe = subscribe;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public void StartMonitorLoop()
        {
            _logger.LogInformation("Monitor Loop is starting.");

            // Run a console user input loop in a background thread
            Task.Run(Monitor, _cancellationToken);
        }

        public async Task Monitor()
        {
            // To start tasks batch enter from Redis console the command - hset subscribeOnFrom tasks:count 30 (where 30 is tasks count - from 10 to 50)

            KeyEvent eventCmdSet = KeyEvent.HashSet; 
            // все ключи положить в константы
            string eventKeyFrom = "subscribeOnFrom"; // ключ для подписки на команду запуска эмулятора сервера
            string eventFieldFrom = "count";

            // заменить ключ и поле на guid, чтобы никто не мог случайно вызвать?
            string eventKeyRun = "task:run"; // ключ и поле для подписки на ключи задач, создаваемые сервером (или эмулятором)
            string eventFieldRun = "ttt";
            // сервер кладёт название поля ключа в заранее обусловленную ячейку ("task:run/count") и тут её можно прочитать
            string eventGuidFieldRun = await _subscribe.FetchGuidFieldTaskRun(eventKeyRun, eventFieldRun);

            _subscribe.SubscribeOnEventFrom(eventKeyFrom, eventFieldFrom, eventCmdSet, eventKeyRun, eventGuidFieldRun);

            // несколько процессов с подпиской на ключ появления задания эмулируют несколько background серверов
            _subscribe.SubscribeOnEventRun(eventKeyRun, eventCmdSet, eventGuidFieldRun, 1); // 1 - номер сервера, потом можно заменить на guid
            _subscribe.SubscribeOnEventRun(eventKeyRun, eventCmdSet, eventGuidFieldRun, 2);

            //string eventKeyAdd = "task:add";
            //_subscribe.SubscribeOnEventAdd(eventKeyAdd, eventCmdSet);

            //KeyEvent eventCmdDel = KeyEvent.Delete;

            //string eventKey = "subscribeOnFrom";

            //_subscribe.SubscribeOnEventFrom(eventKey, eventCmdDel);

            while (IsCancellationNotYet())
            {
                var keyStroke = Console.ReadKey();

                if (keyStroke.Key == ConsoleKey.W)
                {
                    _logger.LogInformation("ConsoleKey was received {KeyStroke}.", keyStroke.Key);
                }
            }
        }

        private bool IsCancellationNotYet ()
        {
            return !_cancellationToken.IsCancellationRequested; // add special key from Redis?
        }
    }
}
