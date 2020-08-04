using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
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
