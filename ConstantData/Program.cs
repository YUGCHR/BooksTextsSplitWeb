using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CachingFramework.Redis;
using CachingFramework.Redis.Contracts.Providers;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using ConstantData.Services;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Shared.Library.Services;
using Shared.Library.Models;

namespace ConstantData
{
    public class Program
    {

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<Program>();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                { // https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration-providers

                    configuration.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    configuration
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
                })

            //.UseContentRoot(Directory.GetCurrentDirectory())
            //.ConfigureAppConfiguration((hostContext, config) =>
            //{
            //    var env = hostContext.HostingEnvironment;

            //    // find the shared folder in the parent folder
            //    string[] paths = { env.ContentRootPath, "..", "SharedSettings" };
            //    string ownPaths = env.ContentRootPath;
            //    var sharedFolder = Path.Combine(paths);
            //    var ownFolder = Path.Combine(ownPaths);

            //    //load the SharedSettings first, so that appsettings.json overrwrites it
            //    // можно убрать shared setting и хранить все константы в локальном appsetting проекта константы
            //    config
            //        //.AddJsonFile(Path.Combine(sharedFolder, "sharedSettings.json"), optional: true)
            //        //.AddJsonFile(Path.Combine(ownFolder, "appsettings.json"), optional: true);
            //        .AddJsonFile("appsettings.json", optional: true)
            //        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            //    config.AddEnvironmentVariables();
            //})
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
                    .WriteTo.File("logs/ConstantData{Date}.txt", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
                    .CreateLogger();

                Logs.Information("The global logger Serilog has been configured.\n");
            })
            .UseDefaultServiceProvider((ctx, opts) => { /* elided for brevity */ })
            .ConfigureServices((hostContext, services) =>
                {
                    try
                    {
                        //ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("redis");
                        ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("localhost");
                        services.AddSingleton<ICacheProviderAsync>(new RedisContext(muxer).Cache);
                        services.AddSingleton<IKeyEventsProvider>(new RedisContext(muxer).KeyEvents);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        Console.WriteLine($"\n\n Redis server did not start: \n + {message} \n");
                        throw;
                    }
                    services.AddSingleton<GenerateThisInstanceGuidService>();
                    services.AddSingleton<ICacheManageService, CacheManageService>();
                    services.AddSingleton<ISharedDataAccess, SharedDataAccess>();
                    services.AddSingleton<IConstantsCollectionService, ConstantsCollectionService>();
                    services.AddSingleton<IOnKeysEventsSubscribeService, OnKeysEventsSubscribeService>();
                    //services.AddSingleton<ConstantNames, constants>();
                    //services.AddSingleton<IInitConstantsService, InitConstantsService>();
                    //services.AddSingleton<ISettingConstantsService, SettingConstantsServiceService>();
                    services.AddSingleton<MonitorLoop>();
                });

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
            monitorLoop.StartMonitorLoop();

            host.Run();
        }
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
}