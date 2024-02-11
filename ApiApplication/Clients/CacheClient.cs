using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Clients.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiApplication.Clients
{
    public class CacheClient : ICacheClient
    {
        private IDistributedCache _cache;

        public object JsonConvert { get; private set; }

        public CacheClient(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetCacheValueAsync<T>(string key, T data, CancellationToken ct, TimeSpan? ttl = null)
        {
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
        }

        public async Task<T> GetCacheValueAsync<T>(string key, CancellationToken ct)
        {
            var redisValue = await _cache.GetStringAsync(key);

            return JsonSerializer.Deserialize<T>(redisValue);
        }
    }
}
