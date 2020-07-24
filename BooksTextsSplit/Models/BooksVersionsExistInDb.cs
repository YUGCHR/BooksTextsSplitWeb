using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class BooksVersionsExistInDb
    {
        public List<SentencesGroupedByLanguageIdGroupedByBookId> AllVersionsOfBookNames { get; set; }
        //public List<TextSentence> AllBookNamesSortedByIds { get; set; }        
    }

    public class SentencesGroupedByLanguageIdGroupedByBookId
    {
        public int BookId { get; set; }
        public IList<SentencesGroupedByLanguageId> BooksDescriptions { get; set; }
    }

    public class SentencesGroupedByLanguageId
    {
        public int LanguageId { get; set; }
        public IList<TextSentence> Sentences { get; set; }
    }
}

