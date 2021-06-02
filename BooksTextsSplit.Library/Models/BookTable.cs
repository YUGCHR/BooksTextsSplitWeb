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
