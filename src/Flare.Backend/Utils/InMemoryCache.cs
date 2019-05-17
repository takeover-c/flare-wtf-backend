using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Flare.Backend.Utils {
    public class InMemoryCache<T> : MemoryCache {
        public InMemoryCache() : base(nameof(T)) {
            
        }

        public Task<T> GetAsync(string key, Func<Task<T>> populator, TimeSpan expire) {
            var task = (Task<T>)Get(key);
            if (task != null)
                return task;

            task = (Task<T>)AddOrGetExisting(key, populator(), DateTimeOffset.Now.Add(expire));
            if (task != null)
                return task;

            return (Task<T>)Get(key);
        }

        public Task<T> GetAsyncNoSet(string key) {
            var task = (Task<T>)Get(key);
            if (task != null)
                return task;

            return Task.FromResult(default(T));
        }
    }
}
