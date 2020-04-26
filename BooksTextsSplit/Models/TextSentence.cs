using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class TextSentence
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "languageId")]
        public int LanguageId { get; set; }

        [JsonProperty(PropertyName = "bookId")]
        public int BookId { get; set; }

        [JsonProperty(PropertyName = "bookName")]
        public string BookName { get; set; }

        [JsonProperty(PropertyName = "chapterId")]
        public int ChapterId { get; set; }

        [JsonProperty(PropertyName = "chapterName")]
        public string ChapterName { get; set; }

        [JsonProperty(PropertyName = "paragraphId")]
        public int ParagraphId { get; set; }

        [JsonProperty(PropertyName = "paragraphName")]
        public string ParagraphName { get; set; }

        [JsonProperty(PropertyName = "sentenceId")]
        public int SentenceId { get; set; }        

        [JsonProperty(PropertyName = "sentenceText")]
        public string SentenceText { get; set; }
    }
}

