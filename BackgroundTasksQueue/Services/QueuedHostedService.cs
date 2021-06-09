using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using BackgroundTasksQueue.Models;
using Shared.Library.Services;
using Shared.Library.Models;

namespace BackgroundTasksQueue.Services
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IOnKeysEventsSubscribeService _subscribe;

        public QueuedHostedService(
            IOnKeysEventsSubscribeService subscribe)
        {
            _subscribe = subscribe;
        }

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<QueuedHostedService>();
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logs.Here().Information("Queued Hosted Service is running. BackgroundProcessing will be called now.");
            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            // подписываемся на ключ кафе - также это точка входа всего
            await _subscribe.SubscribeOnEventRun(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            // тут можно удалить регистрацию сервера на общем ключе серверов
            Logs.Here().Information("Queued Hosted Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}

