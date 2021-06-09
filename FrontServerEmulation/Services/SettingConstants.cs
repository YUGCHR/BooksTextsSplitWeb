using System;
using Microsoft.Extensions.Configuration;

namespace FrontServerEmulation.Services
{
    public interface ISettingConstants
    {
        public int GetRecordActualityLevel { get; }
        public int GetTaskDelayTimeInSeconds { get; }
        public int GetBalanceOfTasksAndProcesses { get; }
        public int GetMaxProcessesCountOnServer { get; }
        public int GetMinBackProcessesServersCount { get; }
        public int GetEventKeyFromTimeDays { get; }
        public int GetEventKeyBackReadinessTimeDays { get; }
        public int GetEventKeyFrontGivesTaskTimeDays { get; }
        public int GetEventKeyBackServerMainTimeDays { get; }
        public int GetEventKeyBackServerAuxiliaryTimeDays { get; }
        public int GetPercentsKeysExistingTimeInMinutes { get; }
        public string GetEventKeyFrom { get; }
        public string GetEventFieldFrom { get; }
        public string GetEventKeyBackReadiness { get; }
        public string GetEventKeyFrontGivesTask { get; }
        public string GetPrefixRequest { get; }
        public string GetPrefixPackage { get; }
        public string GetPrefixTask { get; }
        public string GetPrefixBackServer { get; }
        public string GetPrefixProcessAdd { get; }
        public string GetPrefixProcessCancel { get; }
        public string GetPrefixProcessCount { get; }
        public string GetEventFieldBack { get; }
        public string GetEventFieldFront { get; }
        public string GetEventKeyBacksTasksProceed { get; }
    }

    public class SettingConstants : ISettingConstants
    {
        private readonly int _getRecordActualityLevel;
        private readonly int _getTaskDelayTimeInSeconds;
        private readonly int _getBalanceOfTasksAndProcesses;
        private readonly int _getMaxProcessesCountOnServer;
        private readonly int _getMinBackProcessesServersCount;
        private readonly int _getEventKeyFromTimeDays;
        private readonly int _getEventKeyBackReadinessTimeDays;
        private readonly int _getEventKeyFrontGivesTaskTimeDays;
        private readonly int _getEventKeyBackServerMainTimeDays;
        private readonly int _getEventKeyBackServerAuxiliaryTimeDays;
        private readonly int _getPercentsKeysExistingTimeInMinutes;
        private readonly string _getEventKeyFrom;
        private readonly string _getEventFieldFrom;
        private readonly string _getEventKeyBackReadiness;
        private readonly string _getEventKeyFrontGivesTask;
        private readonly string _getPrefixRequest;
        private readonly string _getPrefixPackage;
        private readonly string _getPrefixTask;
        private readonly string _getPrefixBackServer;
        private readonly string _getPrefixProcessAdd;
        private readonly string _getPrefixProcessCancel;
        private readonly string _getPrefixProcessCount;
        private readonly string _getEventFieldBack;
        private readonly string _getEventFieldFront;
        private readonly string _getEventKeyBacksTasksProceed;

        public SettingConstants(IConfiguration configuration)
        {
            Configuration = configuration;

            string recordActualityLevel = Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("RecordActualityLevel").Value;
            _getRecordActualityLevel = Convert.ToInt32(recordActualityLevel);
            _getTaskDelayTimeInSeconds = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("TaskEmulatorDelayTimeInMilliseconds").Value);
            _getBalanceOfTasksAndProcesses = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("BalanceOfTasksAndProcesses").Value);
            _getMaxProcessesCountOnServer = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("MaxProcessesCountOnServer").Value);
            _getMinBackProcessesServersCount = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("MinBackProcessesServersCount").Value);

            _getEventKeyFromTimeDays = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("RedisKeysTimes").GetSection("eventKeyFromTimeDays").Value);
            _getEventKeyBackReadinessTimeDays = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("RedisKeysTimes").GetSection("eventKeyBackReadinessTimeDays").Value);
            _getEventKeyFrontGivesTaskTimeDays = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("RedisKeysTimes").GetSection("eventKeyFrontGivesTaskTimeDays").Value);
            _getEventKeyBackServerMainTimeDays = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("RedisKeysTimes").GetSection("eventKeyBackServerMainTimeDays").Value);
            _getEventKeyBackServerAuxiliaryTimeDays = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("RedisKeysTimes").GetSection("eventKeyBackServerAuxiliaryTimeDays").Value);
            _getPercentsKeysExistingTimeInMinutes = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("RedisKeysTimes").GetSection("PercentsKeysExistingTimeInMinutes").Value);


            _getEventKeyFrom = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("eventKeyFrom").Value;
            _getEventFieldFrom = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("eventFieldFrom").Value;
            _getEventKeyBackReadiness = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("eventKeyBackReadiness").Value;
            _getEventKeyFrontGivesTask = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("eventKeyFrontGivesTask").Value;

            _getPrefixRequest = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("prefixRequest").Value;
            _getPrefixPackage = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("prefixPackage").Value;
            _getPrefixTask = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("prefixTask").Value;
            _getPrefixBackServer = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("prefixBackServer").Value;

            _getPrefixProcessAdd = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("prefixProcessAdd").Value;
            _getPrefixProcessCancel = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("prefixProcessCancel").Value;
            _getPrefixProcessCount = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("prefixProcessCount").Value;

            _getEventFieldBack = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("eventFieldBack").Value;
            _getEventFieldFront = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("eventFieldFront").Value;
            _getEventKeyBacksTasksProceed = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("eventKeyBacksTasksProceed").Value;

        }

        private IConfiguration Configuration { get; }

        public int GetRecordActualityLevel => _getRecordActualityLevel;
        public int GetTaskDelayTimeInSeconds => _getTaskDelayTimeInSeconds;
        public int GetBalanceOfTasksAndProcesses => _getBalanceOfTasksAndProcesses;
        public int GetMaxProcessesCountOnServer => _getMaxProcessesCountOnServer;
        public int GetMinBackProcessesServersCount => _getMinBackProcessesServersCount;
        public int GetEventKeyFromTimeDays => _getEventKeyFromTimeDays;
        public int GetEventKeyBackReadinessTimeDays => _getEventKeyBackReadinessTimeDays;
        public int GetEventKeyFrontGivesTaskTimeDays => _getEventKeyFrontGivesTaskTimeDays;
        public int GetEventKeyBackServerMainTimeDays => _getEventKeyBackServerMainTimeDays;
        public int GetEventKeyBackServerAuxiliaryTimeDays => _getEventKeyBackServerAuxiliaryTimeDays;
        public int GetPercentsKeysExistingTimeInMinutes => _getPercentsKeysExistingTimeInMinutes;
        public string GetEventKeyFrom => _getEventKeyFrom;
        public string GetEventFieldFrom => _getEventFieldFrom;
        public string GetEventKeyBackReadiness => _getEventKeyBackReadiness;
        public string GetEventKeyFrontGivesTask => _getEventKeyFrontGivesTask;
        public string GetPrefixRequest => _getPrefixRequest;
        public string GetPrefixPackage => _getPrefixPackage;
        public string GetPrefixTask => _getPrefixTask;
        public string GetPrefixBackServer => _getPrefixBackServer;
        public string GetPrefixProcessAdd => _getPrefixProcessAdd;
        public string GetPrefixProcessCancel => _getPrefixProcessCancel;
        public string GetPrefixProcessCount => _getPrefixProcessCount;
        public string GetEventFieldBack => _getEventFieldBack;
        public string GetEventFieldFront => _getEventFieldFront;
        public string GetEventKeyBacksTasksProceed => _getEventKeyBacksTasksProceed;

    }
}