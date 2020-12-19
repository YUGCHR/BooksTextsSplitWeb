using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using System.Text.Json;
using Newtonsoft.Json;

namespace BooksTextsSplit.Models
{
    public class TaskUploadPercents
    {
        [JsonProperty(PropertyName = "isTaskRunning")]
        public bool IsTaskRunning { get; set; }

        [JsonProperty(PropertyName = "currentTaskGuid")]
        public string CurrentTaskGuid { get; set; }

        [JsonProperty(PropertyName = "currentUploadingBookId")]
        public int CurrentUploadingBookId { get; set; }

        [JsonProperty(PropertyName = "currentUploadingLanguageId")]
        public int CurrentUploadingLanguageId { get; set; }

        [JsonProperty(PropertyName = "currentUploadingVersion")]
        public int CurrentUploadingVersion { get; set; }

        [JsonProperty(PropertyName = "doneInProcents")]
        public int DoneInPercents { get; set; } //don't use

        [JsonProperty(PropertyName = "currentUploadingRecord")]
        public int CurrentUploadingRecord { get; set; }

        [JsonProperty(PropertyName = "currentUploadedRecordRealTime")]
        public int CurrentUploadedRecordRealTime { get; set; }

        [JsonProperty(PropertyName = "totalUploadedRealTime")]
        public int TotalUploadedRealTime { get; set; }

        [JsonProperty(PropertyName = "recordrsTotalCount")]
        public int RecordsTotalCount { get; set; }

        [JsonProperty(PropertyName = "redisKey")]
        public string RedisKey { get; set; }

        [JsonProperty(PropertyName = "fieldKeyPercents")]
        public string FieldKeyPercents { get; set; }

        [JsonProperty(PropertyName = "fieldKeyState")]
        public string FieldKeyState { get; set; } //don't use
        
        [JsonProperty(PropertyName = "keysExistingTime")]
        public TimeSpan? KeysExistingTime { get; set; }

        public TextSentence ShallowCopy()
        {
            return (TextSentence)this.MemberwiseClone();
        }
    }
}