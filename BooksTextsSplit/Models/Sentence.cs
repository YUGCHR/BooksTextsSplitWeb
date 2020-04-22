using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class Sentence
    {
        public int ID { get; set; }
        public int LanguageID { get; set; }
        public int ChapterID { get; set; }
        public int ParagraphID { get; set; }
        public int SentenceNumber { get; set; }
        public string SentenceName { get; set; }
    }
}
