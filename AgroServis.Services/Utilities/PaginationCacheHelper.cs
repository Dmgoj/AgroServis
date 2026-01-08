using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services.Utilities
{
    public static class CacheHelper
    {
        public static void InvalidatePaginationCaches(
            IMemoryCache cache,
            ILogger logger,
            string entityName,
            int maxPages = 10,
            int[] pageSizes = null)
        {
            pageSizes ??= new[] { 10, 20, 50 };

            logger.LogDebug("Invalidating {EntityName} pagination caches", entityName);

            foreach (var pageSize in pageSizes)
            {
                for (int page = 1; page <= maxPages; page++)
                {
                    var cacheKey = $"{entityName}Page_{page}_Size_{pageSize}";
                    cache.Remove(cacheKey);
                }
            }
        }
    }
}