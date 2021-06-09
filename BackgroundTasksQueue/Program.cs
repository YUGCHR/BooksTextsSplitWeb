using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using CachingFramework.Redis;
using CachingFramework.Redis.Contracts.Providers;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using BackgroundTasksQueue.Services;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Shared.Library.Services;

namespace BackgroundTasksQueue
{
    public class Program
    {
        private static Serilog.ILogger Logs => Serilog.Log.ForContext<Program>();

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            //var monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
            //monitorLoop.StartMonitorLoop();

            host.WaitForShutdownAsync();

            host.Run();
            Log.Information("The global logger has been closed and flushed");
            Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                IHostEnvironment env = hostContext.HostingEnvironment;

                // find the shared folder in the parent folder
                //string[] paths = { env.ContentRootPath, "..", "SharedSettings" };
                //var sharedFolder = Path.Combine(paths);

                //load the SharedSettings first, so that appsettings.json overrwrites it
                config
                    //.AddJsonFile(Path.Combine(sharedFolder, "sharedSettings.json"), optional: true)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

                config.AddEnvironmentVariables();
            })
            .ConfigureLogging((ctx, sLog) =>
            {
                //var seriLog = new LoggerConfiguration()
                //    .WriteTo.Console()
                //    .CreateLogger();

                //var outputTemplate = "{Timestamp:HH:mm} [{Level:u3}] ({ThreadId}) {Message}{NewLine}{Exception}";
                //var outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}in method {MemberName} at {FilePath}:{LineNumber}{NewLine}{Exception}{NewLine}";
                string outputTemplate = "{NewLine}[{Timestamp:HH:mm:ss} {Level:u3} ({ThreadId}) {SourceContext}.{MemberName} - {LineNumber}] {NewLine} {Message} {NewLine} {Exception}";

                //seriLog.Information("Hello, Serilog!");

                //Log.Logger = seriLog;
                // попробовать менять уровень вывода логгера через переменную
                // const LogEventLevel loggerLevel = LogEventLevel.Debug;
                // https://stackoverflow.com/questions/25477415/how-can-i-reconfigure-serilog-without-restarting-the-application
                // https://stackoverflow.com/questions/51389550/serilog-json-config-logginglevelswitch-access
                const LogEventLevel loggerLevel = LogEventLevel.Information;
                Log.Logger = new LoggerConfiguration()
                    .Enrich.With(new ThreadIdEnricher())
                    .Enrich.FromLogContext()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console(restrictedToMinimumLevel: loggerLevel, outputTemplate: outputTemplate, theme: AnsiConsoleTheme.Literate) //.Verbose .Debug .Information .Warning .Error .Fatal
                    .WriteTo.File("logs/BackgroundTasksQueue{Date}.txt", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
                    .CreateLogger();

                Logs.Information("The global logger Serilog has been configured.\n");
            })
            .UseDefaultServiceProvider((ctx, opts) => { /* elided for brevity */ })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ILogger>(Log.Logger);
                try
                {
                    ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("localhost");
                    services.AddSingleton<ICacheProviderAsync>(new RedisContext(muxer).Cache);
                    //services.AddSingleton<IPubSubProvider>(new RedisContext(muxer).PubSub);
                    services.AddSingleton<IKeyEventsProvider>(new RedisContext(muxer).KeyEvents);
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    //Console.WriteLine($"\n\n Redis server did not start: \n + {message} \n");
                    Logs.Fatal("Redis server did not find: \n {@EM} \n\n", new { ExceptionMessage = message });
                    throw;
                }
                services.AddSingleton<GenerateThisInstanceGuidService>();
                services.AddSingleton<ICacheManageService, CacheManageService>();
                services.AddSingleton<ISharedDataAccess, SharedDataAccess>();
                services.AddHostedService<QueuedHostedService>();
                services.AddSingleton<ISettingConstants, SettingConstantsService>();
                services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>(); 
                services.AddSingleton<IBackgroundTasksService, BackgroundTasksService>();
                services.AddSingleton<IOnKeysEventsSubscribeService, OnKeysEventsSubscribeService>();
                services.AddSingleton<ITasksPackageCaptureService, TasksPackageCaptureService>();
                services.AddSingleton<ITasksBatchProcessingService, TasksBatchProcessingService>();
                services.AddSingleton<ITasksProcessingControlService, TasksProcessingControlService>();
            });
    }

    internal class ThreadIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ThreadId", Thread.CurrentThread.ManagedThreadId));
        }
    }

    public static class LoggerExtensions
    {
        // https://stackoverflow.com/questions/29470863/serilog-output-enrich-all-messages-with-methodname-from-which-log-entry-was-ca/46905798

        public static ILogger Here(this ILogger logger, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        //[CallerFilePath] string sourceFilePath = "",
        {
            return logger.ForContext("MemberName", memberName).ForContext("LineNumber", sourceLineNumber);
            //.ForContext("FilePath", sourceFilePath)
        }
    }

    public static class RandomProvider
    {
        private static readonly Random Rnd = new(Guid.NewGuid().ToString().GetHashCode());
        private static readonly object Sync = new();

        public static int Next(int min, int max)
        {
            lock (Sync)
            {
                return Rnd.Next(min, max);
            }
        }
    }

    // BackServers
    // кубик всё время выбрасывает ноль - TasksPackageCaptureService.DiceRoll corrected
    // разделить подписку и обработку для завершения задач -done
    // всё вызывать из BackgroundTaskQueue, монитор оставить пустой, только с клавишей - done
    // процесс всё время создаётся один - нет, каждый раз ещё один
    // процесс при каждом вбросе создаётся новый или старые учитываются?
    // как учитывать занятые процессы и незанятые - непонятно
    // хранить процессы в листе класса
    // 
    // ----- вы сейчас находитесь здесь -----
    // 
    // можно дополнительно иногда проверять по таймеру завершение пакета
    // и ещё можно параллельно проверять загрузку процессов - если появились свободные процессы, пора идти искать новый пакет
    // собирать отчёт о выполнении задач и показывать в конце - количество пакетов, задач в каждом, среднее время единичной задачи в пакете, время каждого пакета и общее время
    // неправильно обрабатывается условие в while в Monitor
    // при завершении сервера успеть удалить своё поле из ключа регистрации серверов - обработать cancellationToken
    // 
    // --- перспективный план ---
    // *1* словарь констант - готово
    // *2* перезагрузка констант между пакетами - готово
    // блокировать обе подписки одним флагом и потом обе проверять, но константы проверять первыми
    // рассмотреть вариант обновления констант после каждого пакета
    // но не забыть вариант простоя без пакетов
    // ----- вы сейчас находитесь здесь -----
    // можно добавить переключатель автоматически/вручную и ручную (+/-) регулировку количества процессов в настройках веб-интерфейса
    // *3* отслеживание упавших процессов
    // разделение функций - один модуль уточняет время проверки, а второй оббегает все задачи и проверяет состояние, сравнивая его с расчётным
    // по расчёту каждый тик - цикл опроса - должна заканчиваться одна задача
    // если за тик задача не закончилась, ждём второй, третий, пятый (в константах) и перезапускаем
    // перезапуск - много вариантов - упал процесс, повисла задача, что ещё?
    // как проверить наличие живых процессов? только положить в очередь тестовую задачу
    // если задача повисла, то процесс тоже надо удалить
    // то есть, при заминке надо удалить все процессы, создать новые и запустить остававшиеся задачи
    // повторить 2-3-5 раз? делать процесс на один больше нормы, вдруг поможет
    // при перезапуске задач надо обновить ключ, по которому отслеживается окончание выполнения пакета
    // если не получилось, сообщить и снять пакет с выполнения, задаче присвоить признак "вечно живая"
    // *4* отслеживание упавших пакетов

    // Constants
    // добавить в набор констант признак базового и версию, в базу записать название ключа, в версию счётчик обновлений
    // можно убрать shared setting и хранить все константы в локальном appsetting проекта константы
    // установить своё время для каждого ключа, можно вместе с названием ключа - словарь
    // сделать константы ключей в виде словаря - строка/время существования ключа
    // везде использовать имя ключа с типом словаря и только в последнем методе раскрывать и записывать
    // подумать, как обновятся константы в сервере по требованию
    // при сохранении новых значений генерировать событие, чтобы все взяли новые константы
    // ----- вы сейчас находитесь здесь -----
    // добавить веб-интерфейс с возможностью устанавливать константы - страница setting

    // FrontEmulator
    // ----- вы сейчас находитесь здесь -----
    // сторожевой таймер, контролирующий зависание сервера
    // можно класть время задержки цикла в условие задачи из эмулятора, эмулятор точно так же обновит константы в свободное время
    // 
    // 

    // вариант генерации уникального номера сервера (сохранить)
    //public static class ThisBackServerGuid
    //{
    //    static ThisBackServerGuid()
    //    {
    //        thisBackServerGuid = Guid.NewGuid().ToString();
    //    }

    //    private static readonly string thisBackServerGuid;

    //    public static string GetThisBackServerGuid()
    //    {
    //        return thisBackServerGuid;
    //    }
    //}
}
