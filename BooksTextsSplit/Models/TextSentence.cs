using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using System.Text.Json;
using Newtonsoft.Json;

namespace BooksTextsSplit.Models
{
    public class TextSentence
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "recordActualityLevel")]
        public int RecordActualityLevel { get; set; } // версия структуры данных в базе, чтобы легко отделить от старых загрузок

        [JsonProperty(PropertyName = "languageId")]
        public int LanguageId { get; set; }

        [JsonProperty(PropertyName = "uploadVersion")]
        public int UploadVersion { get; set; } // means BooksPairId - pair of English and Russian book

        [JsonProperty(PropertyName = "bookId")]
        public int BookId { get; set; } // means BooksPairId - pair of English and Russian book

        [JsonProperty(PropertyName = "alignedSentencesProportion")]
        public int AlignedSentencesProportion { get; set; } // процент выровненных предложений в обоих языках в текстах книг

        [JsonProperty(PropertyName = "totalBookCounts")]
        public TotalBooksCounts TotalBookCounts { get; set; }

        [JsonProperty(PropertyName = "bookProperties")]
        public BookPropertiesInLanguage BookProperties { get; set; }

        [JsonProperty(PropertyName = "chapterId")]
        public int ChapterId { get; set; }

        [JsonProperty(PropertyName = "chapterName")]
        public string ChapterName { get; set; }

        [JsonProperty(PropertyName = "inChapterParagraphsCount")]
        public int InChapterParagraphsCount { get; set; } // how many paragraphs in the chapter

        [JsonProperty(PropertyName = "inChapterSentencesCount")]
        public int InChapterSentencesCount { get; set; } // how many sentences in all paragraphs in the chapter

        [JsonProperty(PropertyName = "bookContent")]
        public List<BookContentInChapters> BookContentInChapter { get; set; }

        public class TotalBooksCounts
        {
            [JsonProperty(PropertyName = "inBookChaptersCount")]
            public int InBookChaptersCount { get; set; } // how many chapters in the book (one languageId)

            [JsonProperty(PropertyName = "inBookParagraphsCount")]
            public int InBookParagraphsCount { get; set; } // how many paragraphs in all chapters in the book (one languageId)

            [JsonProperty(PropertyName = "inBookSentencesCount")]
            public int InBookSentencesCount { get; set; } // sentences in the same way
        }

        public TextSentence ShallowCopy()
        {
            return (TextSentence)this.MemberwiseClone();
        }

        public class BookPropertiesInLanguage
        {
            [JsonProperty(PropertyName = "authorNameId")]
            public int AuthorNameId { get; set; }

            [JsonProperty(PropertyName = "authorName")]
            public string AuthorName { get; set; }

            [JsonProperty(PropertyName = "bookNameId")]
            public int BookNameId { get; set; }

            [JsonProperty(PropertyName = "bookName")]
            public string BookName { get; set; }

            [JsonProperty(PropertyName = "bookAnnotation")]
            public string BookAnnotation { get; set; }

        }

        public class BookContentInChapters
        {
            [JsonProperty(PropertyName = "paragraphId")]
            public int ParagraphId { get; set; }

            [JsonProperty(PropertyName = "paragraphName")]
            public string ParagraphName { get; set; }            

            [JsonProperty(PropertyName = "bookContentInParagraph")]
            public List<BookContentInParagraphs> BookContentInParagraph { get; set; }

            public class BookContentInParagraphs
            {
                [JsonProperty(PropertyName = "bookSentenceId")]
                public int BookSentenceId { get; set; } // end-to-end numbering of all books sentences

                [JsonProperty(PropertyName = "sentenceId")]
                public int SentenceId { get; set; }

                [JsonProperty(PropertyName = "inParagraphSentencesCounts")]
                public int InParagraphSentencesCounts { get; set; }

                [JsonProperty(PropertyName = "sentenceText")]
                public string SentenceText { get; set; }
            }
        }
    }







}

    //ideally this would be done in a config file at app startup
    //Mapper.Initialize(cfg => cfg.CreateMap<Person, Person>());

    //create the deep copy using AutoMapper
    //var deepCopy = Mapper.Map<Person>(originalPerson);


