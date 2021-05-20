namespace BooksTextsSplit.Library.Models
{
    public class TotalCount
    {
        public TotalCount(int count)
        {
            sentencesCount = count;
        }
        public int sentencesCount { get; private set; }  // { get { return f_sentencesCount; } set { f_sentencesCount = value; } }

        //public int f_sentencesCount;
    }
}
