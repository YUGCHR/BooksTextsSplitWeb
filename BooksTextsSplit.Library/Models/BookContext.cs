using Microsoft.EntityFrameworkCore;

namespace BooksTextsSplit.Library.Models
{
    public class BookContext : DbContext
    {
        public BookContext(DbContextOptions<BookContext> options)
            : base(options)
        {
        }

        public DbSet<TextSentence> BookTexts { get; set; }
        
    }
}
