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
    }

    public class SettingConstants : ISettingConstants
    {
        private int _getRecordActualityLevel;

        public SettingConstants(IConfiguration configuration)
        {
            Configuration = configuration;

            string recordActualityLevel = Configuration.GetSection("SettingConstants").GetSection("Constants").GetSection("RecordActualityLevel").Value;
            _getRecordActualityLevel = Convert.ToInt32(recordActualityLevel);
        }
        private IConfiguration Configuration { get; }

        public int GetRecordActualityLevel => _getRecordActualityLevel;
      
    }
}
