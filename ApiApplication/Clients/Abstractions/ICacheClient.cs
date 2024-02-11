using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Clients.Abstractions
{
    public interface ICacheClient
    {
        Task SetCacheValueAsync<T>(string key, T data, CancellationToken ct, TimeSpan? ttl = null);
        Task<T> GetCacheValueAsync<T>(string key, CancellationToken ct);
    }
}
