using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class TextSentenceFlat
    {
        [JsonProperty(PropertyName = "languageId")]
        public int LanguageId { get; set; }

        [JsonProperty(PropertyName = "chapterId")]
        public int ChapterId { get; set; }

        [JsonProperty(PropertyName = "chapterName")]
        public string ChapterName { get; set; }

        [JsonProperty(PropertyName = "paragraphId")]
        public int ParagraphId { get; set; }

        [JsonProperty(PropertyName = "paragraphName")]
        public string ParagraphName { get; set; }

        [JsonProperty(PropertyName = "bookSentenceId")]
        public int BookSentenceId { get; set; } // end-to-end numbering of all books sentences

        [JsonProperty(PropertyName = "sentenceId")]
        public int SentenceId { get; set; }

        [JsonProperty(PropertyName = "sentenceText")]
        public string SentenceText { get; set; }
    }
}
