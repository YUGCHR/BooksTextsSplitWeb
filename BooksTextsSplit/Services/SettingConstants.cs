using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Services
{
    public interface ISettingConstants
    {
        public int GetRecordActualityLevel { get; }
        public int GetTaskDelayTimeInSeconds { get; }
        public int GetPersentsKeysExistingTimeInMinutes { get; }
    }

    public class SettingConstants : ISettingConstants
    {
        private int _getRecordActualityLevel;
        private int _getTaskDelayTimeInSeconds;
        private int _getPersentsKeysExistingTimeInMinutes;

        public SettingConstants(IConfiguration configuration)
        {
            Configuration = configuration;

            string recordActualityLevel = Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("RecordActualityLevel").Value;
            _getRecordActualityLevel = Convert.ToInt32(recordActualityLevel);
            
            _getTaskDelayTimeInSeconds = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("TaskDelayTimeInSeconds").Value);

            _getPersentsKeysExistingTimeInMinutes = Convert.ToInt32(Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("PersentsKeysExistingTimeInMinutes").Value);


        }
        private IConfiguration Configuration { get; }

        public int GetRecordActualityLevel => _getRecordActualityLevel;
        public int GetTaskDelayTimeInSeconds => _getTaskDelayTimeInSeconds;
        public int GetPersentsKeysExistingTimeInMinutes => _getPersentsKeysExistingTimeInMinutes;
      
    }
}
