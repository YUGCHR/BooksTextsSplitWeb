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

        [JsonProperty(PropertyName = "uploadVersion")]
        public int UploadVersion { get; set; } // means BooksPairId - pair of English and Russian book

        [JsonProperty(PropertyName = "bookId")]
        public int BookId { get; set; } // means BooksPairId - pair of English and Russian book

        [JsonProperty(PropertyName = "authorNameId")]
        public int AuthorNameId { get; set; }

        [JsonProperty(PropertyName = "authorName")]
        public string AuthorName { get; set; }

        [JsonProperty(PropertyName = "bookNameId")]
        public int BookNameId { get; set; }

        [JsonProperty(PropertyName = "bookName")]
        public string BookName { get; set; }

        [JsonProperty(PropertyName = "bookSentenceId")]
        public int BookSentenceId { get; set; } // end-to-end numbering of all books sentences

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

        public TextSentence ShallowCopy()
        {
            return (TextSentence)this.MemberwiseClone();
        }
    }

    //ideally this would be done in a config file at app startup
    //Mapper.Initialize(cfg => cfg.CreateMap<Person, Person>());

    //create the deep copy using AutoMapper
    //var deepCopy = Mapper.Map<Person>(originalPerson);
}

