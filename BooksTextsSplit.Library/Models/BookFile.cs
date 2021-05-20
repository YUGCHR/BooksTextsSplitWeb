using Newtonsoft.Json;

namespace BooksTextsSplit.Library.Models
{
    public class BookFile
    {
        [JsonProperty(PropertyName = "bookFileId")]
        public string BookFileId { get; set; }

        [JsonProperty(PropertyName = "bookLanguageID")]
        public int BookLanguageID { get; set; }

        [JsonProperty(PropertyName = "bookFilePath")]
        public string BookFilePath { get; set; }

        [JsonProperty(PropertyName = "bookFileName")]
        public string BookFileName { get; set; }

        [JsonProperty(PropertyName = "bookFileContent")]
        public string BookFileContent { get; set; }
    }
}
