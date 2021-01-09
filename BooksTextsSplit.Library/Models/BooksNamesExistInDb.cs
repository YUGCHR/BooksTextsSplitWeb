using System.Collections.Generic;

namespace BooksTextsSplit.Library.Models
{
    public class BooksNamesExistInDb
    {
        public List<BooksNamesSortByLanguageIdSortByBookId> BooksNamesIds { get; set; }
    }

    public class BooksNamesSortByLanguageIdSortByBookId
    {
        public int BookId { get; set; }
        public List<BooksNamesSortByLanguageId> AllBookDescriptions { get; set; }
        public List<BooksNamesSortByLanguageId> BooksDescriptions { get; set; } // will not be used, for compatibility only
    }

    public class BooksNamesSortByLanguageId
    {
        public int LanguageId { get; set; }        
        public List<BookVersionsTotaICount> BookVersionsOfBookId { get; set; }
        public TextSentence Sentence { get; set; } // will not be used, for compatibility only
    }

    public class BookVersionsTotaICount
    {
        public int UploadVersion { get; set; }
        public TextSentence.TotalBooksCounts BookVersionCounts { get; set; }
        public TextSentence.BookPropertiesInLanguage BookDescriptionDetails { get; set; } // можно сохранять общее (одиночное, не массив) название для всех версий

    }
}
