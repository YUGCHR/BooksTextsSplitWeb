using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Library.Models
{
    public class BookTable
    {
        [JsonProperty(PropertyName = "bookGuid")]
        public string BookGuid { get; set; }

        [JsonProperty(PropertyName = "bookId")]
        public int BookId { get; set; }

        [JsonProperty(PropertyName = "uploadVersion")]
        public int UploadVersion { get; set; }

        // потом добавить ещё уровень готовности книги в процентах
        [JsonProperty(PropertyName = "recordActualityLevel")]
        public int RecordActualityLevel { get; set; }

        // ключ "префикс textSentences: bookId: + номер книги +:+ префикс uploadVersion: + версия"
        // в этом ключе хранится собственно пара книг по главам (оба языка)
        [JsonProperty(PropertyName = "textSentencesKey")]
        public string TextSentencesKey { get; set; }

        //Legacy

        public List<UploadVersionContent> UploadVersions { get; set; }

        public class UploadVersionContent
        {
            [JsonProperty(PropertyName = "uploadVersion")]
            public int UploadVersion { get; set; }

            [JsonProperty(PropertyName = "recordActualityLevel")]
            public int RecordActualityLevel { get; set; }

            public List<ChaptersPair> ChaptersPairs { get; set; }

            public class ChaptersPair
            {
                public string ChaptersKey { get; set; }
                public int ChapterNumber { get; set; }
            }
        }
    }
}
