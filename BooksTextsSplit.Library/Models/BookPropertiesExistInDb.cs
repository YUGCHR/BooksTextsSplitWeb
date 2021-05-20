using System.Collections.Generic;

namespace BooksTextsSplit.Library.Models
{
    public class BookPropertiesExistInDb
    {
        public int BookId { get; set; }
        public List<BookVersionPropertiesInLanguage> BookVersionsLanguageInBook { get; set; }
    }

    public class BookVersionPropertiesInLanguage
    {
        public int LanguageId { get; set; }
        public List<BookVersionProperties> BookVersionsInLanguage { get; set; }
    }

    public class BookVersionProperties
    {
        public int UploadVersion { get; set; }
        public TextSentence.TotalBooksCounts BookVersionCounts { get; set; }
        public TextSentence.BookPropertiesInLanguage BookDescriptionDetails { get; set; }
    }
}

