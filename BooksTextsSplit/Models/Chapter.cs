using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class Chapter
    {
        public int ID { get; set; }
        public int LanguageID { get; set; }
        public int ChapterNumber { get; set; }
        public string ChapterName { get; set; }
    }
}
