using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CacheMoney
{
    public class AutoEvictingCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, CacheObject> _cache;
        private readonly Timer _evictionTimer;

        /// <summary>
        ///     Creates a new instance of the AutoEvictingCache
        /// </summary>
        /// <param name="cachedObjectLifetime">How long objects should be kept in cache for after they are added</param>
        /// <param name="evictionInterval">How often the automatic eviction process should run</param>
        public AutoEvictingCache(int cachedObjectLifetime, int evictionInterval = 300000)
        {
            CachedObjectLifetime = cachedObjectLifetime;
            _evictionTimer = new Timer(EvictObjects, this, evictionInterval, evictionInterval);
            _cache = new ConcurrentDictionary<string, CacheObject>();
        }

        private int CachedObjectLifetime { get; set; }

        public void Dispose()
        {
            _evictionTimer.Dispose();
        }

        public T Get<T>(string key) where T : class
        {
            if (typeof(T) == typeof(IDisposable))
                throw new NotSupportedException("Cannot handle disposable objects");

            CacheObject result;
            if (_cache.TryGetValue(key, out result))
            {
                return (T) result.Object;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type of object to get, CANNOT implement IDisposable</typeparam>
        /// <param name="key"></param>
        /// <param name="createAction"></param>
        /// <returns></returns>
        public T GetOrAdd<T>(string key, Func<T> createAction)
        {
            if (typeof(T) == typeof(IDisposable))
                throw new NotSupportedException("Cannot handle disposable objects");

            return (T) _cache.GetOrAdd(key, s => new CacheObject(createAction())).Object;
        }

        private void EvictObjects(object state)
        {
            DateTime currentTime = DateTime.Now;
            DateTime evictBefore = currentTime.Subtract(TimeSpan.FromMilliseconds(CachedObjectLifetime));
            foreach (var item in _cache)
            {
                if (item.Value.Created < evictBefore)
                {
                    CacheObject dest;
                    _cache.TryRemove(item.Key, out dest);
                }
            }
        }
    }
}