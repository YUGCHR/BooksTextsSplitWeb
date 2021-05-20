using System;

namespace BackgroundTasksQueue.Helpers
{
    public static class CacheKeyFormer
    {
        public static string KeyBaseAddLanguageIdBookId(this string keyBase, int languageId, int bookId)
        {
            if (keyBase == null)
            {
                Console.WriteLine("\n KeyBaseAddLanguageIdBookId reports - keyBase is undefined ");
                return null;
            }
            string resultKey = $"{keyBase}{languageId}:{bookId}";
            return resultKey;

        }
        
        public static string KeyBaseAddLanguageId(this string keyBase, int languageId)
        {
            if (keyBase == null)
            {
                Console.WriteLine("\n KeyBaseAddLanguageId reports - keyBase is undefined ");
                return null;
            }
            string resultKey = $"{keyBase}{languageId}";
            return resultKey;
        }

        public static string KeyBaseRedisKey(this string keyBase, string redisKeyNum)
        {
            if (keyBase == null)
            {
                Console.WriteLine("\n KeyBaseRedisKey reports - keyBase is undefined ");
                return null;
            }
            string resultKey = $"{keyBase}:{redisKeyNum}";
            return resultKey;
        }
    }
}
