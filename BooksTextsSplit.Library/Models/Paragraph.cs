namespace BooksTextsSplit.Library.Models
{
    public class Paragraph
    {
        public int ID { get; set; }
        public int LanguageID { get; set; }
        public int ChapterID { get; set; }
        public int ParagraphNumber { get; set; }
        public string ParagraphName { get; set; }
    }
}
