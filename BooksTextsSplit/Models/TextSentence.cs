using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class TextSentence
    {
        public string Id { get; set; }

        public int LanguageId { get; set; }

        public string sentenceText { get; set; }        
    }
}

