using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class BookFile
    {
        public int BookLanguageID { get; set; }
        public string BookFilePath { get; set; }
        public string BookFileType { get; set; }
        public string BookFileContent { get; set; }
    }
}
