using Microsoft.Extensions.Configuration;
using System;

namespace BooksTextsSplit.Library.Services
{
    public interface ISettingConstants
    {
        public int GetRecordActualityLevel { get; }
        public int GetTaskDelayTimeInSeconds { get; }
        public int GetPercentsKeysExistingTimeInMinutes { get; }
        public string GetKeyBookId { get; }
        public string GetKeyLanguageId { get; }
        public string GetKeyAllNumbers { get; }
        public string GetKeyBookIdAction { get; }
        public string GetKeyTaskPercents { get; }
        public string GetKeyIsTaskRunning { get; }
    }

    public class SettingConstants : ISettingConstants
    {
        private readonly int _getRecordActualityLevel;
        private readonly int _getTaskDelayTimeInSeconds;
        private readonly int _getPercentsKeysExistingTimeInMinutes;
        private readonly string _getKeyBookId;
        private readonly string _getKeyLanguageId;
        private readonly string _getKeyAllNumbers;
        private readonly string _getKeyBookIdAction;
        private readonly string _getKeyTaskPercents;
        private readonly string _getKeyIsTaskRunning;

        public SettingConstants(IConfiguration configuration)
        {
            Configuration = configuration;

            string recordActualityLevel = Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("RecordActualityLevel").Value;
            _getRecordActualityLevel = Convert.ToInt32(recordActualityLevel);
            _getTaskDelayTimeInSeconds = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("TaskDelayTimeInSeconds").Value);
            _getPercentsKeysExistingTimeInMinutes = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("PercentsKeysExistingTimeInMinutes").Value);
            _getKeyBookId = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("keyBookId").Value;
            _getKeyLanguageId = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("keyLanguageId").Value;
            _getKeyAllNumbers = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("keyAllNumbers").Value;
            _getKeyBookIdAction = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("keyBookIdAction").Value;
            _getKeyTaskPercents = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("keyTaskPercents").Value;
            _getKeyIsTaskRunning = Configuration.GetSection("SettingConstants").GetSection("RedisKeys").GetSection("keyIsTaskRunning").Value;

        }
        private IConfiguration Configuration { get; }

        public int GetRecordActualityLevel => _getRecordActualityLevel;
        public int GetTaskDelayTimeInSeconds => _getTaskDelayTimeInSeconds;
        public int GetPercentsKeysExistingTimeInMinutes => _getPercentsKeysExistingTimeInMinutes;
        public string GetKeyBookId => _getKeyBookId;
        public string GetKeyLanguageId => _getKeyLanguageId;
        public string GetKeyAllNumbers => _getKeyAllNumbers;
        public string GetKeyBookIdAction => _getKeyBookIdAction;
        public string GetKeyTaskPercents => _getKeyTaskPercents;
        public string GetKeyIsTaskRunning => _getKeyIsTaskRunning;
      
    }
}
