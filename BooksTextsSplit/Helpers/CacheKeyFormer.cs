using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Helpers
{
    public static class CacheKeyFormer
    {
        public static string KeyBaseAddLanguageIdBookId(this string keyBase, int languageId, int bookId)
        {
            if (keyBase == null)
            {
                Console.WriteLine("\n\n keyBase is undefined ");
                return null;
            }
            string resultKey = keyBase + languageId.ToString() + ":" + bookId.ToString();
            return resultKey;

        }public static string KeyBaseAddLanguageId(this string keyBase, int languageId)
        {
            if (keyBase == null)
            {
                Console.WriteLine("\n\n keyBase is undefined ");
                return null;
            }
            string resultKey = keyBase + languageId.ToString();
            return resultKey;
        }
    }
}
