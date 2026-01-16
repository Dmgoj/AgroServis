using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AgroServis.Services.Utilities
{
    public static class CacheVersionHelper
    {
        public static int GetVersion(IMemoryCache cache, string entityName)
        {
            var key = $"{entityName}_CacheVersion";

            if (!cache.TryGetValue(key, out int version))
            {
                version = 1;

                cache.Set(
                    key,
                    version,
                    new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove }
                );
            }

            return version;
        }

        public static void BumpVersion(IMemoryCache cache, string entityName, ILogger logger)
        {
            var key = $"{entityName}_CacheVersion";

            var version = GetVersion(cache, entityName) + 1;

            cache.Set(
                key,
                version,
                new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove }
            );

            logger.LogInformation(
                "{Entity} cache version bumped to {Version}",
                entityName,
                version
            );
        }
    }
}