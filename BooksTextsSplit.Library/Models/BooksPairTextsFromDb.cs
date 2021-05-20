using System.Collections.Generic;

namespace BooksTextsSplit.Library.Models
{
    public class BooksPairTextsFromDb
    {
        public IList<BooksPairTextsGroupByLanguageId> SelectedBooksPairTexts { get; set; }
    }

    public class BooksPairTextsGroupByLanguageId
    {
        public int LanguageId { get; set; }
        public IList<TextSentence> Sentences { get; set; }
    }
}
