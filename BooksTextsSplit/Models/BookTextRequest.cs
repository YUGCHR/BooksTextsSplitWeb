using System.Collections.Generic;
using BooksTextsSplit.Models;

namespace BooksTextsSplit.Models
{
    public class BookTextRequest
    {
        public int LanguageId { get; set; }
        public List<TextSentence> Text { get; set; }
    }
}
