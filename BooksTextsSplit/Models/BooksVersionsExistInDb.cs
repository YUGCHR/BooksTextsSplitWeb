using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class BooksVersionsExistInDb
    {
        public IList<SelectedBookIdGroupByLanguageId> SelectedBookIdAllVersions { get; set; }
    }

    public class SelectedBookIdGroupByLanguageId
    {
        public int LanguageId { get; set; }
        public IList<TextSentence> Sentences { get; set; }
    }
}

