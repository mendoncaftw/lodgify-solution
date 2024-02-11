using System;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Clients.Abstractions;
using ApiApplication.TypedExceptions;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ProtoDefinitions;

namespace ApiApplication.Clients
{
    public class CachedApiClientGrpc : ApiClientGrpc
    {
        private const string ALL_MOVIES_KEY = "movies:all";
        private const string MOVIE_KEY = "movies:{0}";
        private ICacheClient _cache;
        private readonly TimeSpan _ttl;

        public CachedApiClientGrpc(MoviesApi.MoviesApiClient client,
                                   ICacheClient cacheClient,
                                   IOptions<AppSettings> settings) : base(client)
        {
            _cache = cacheClient;
            _ttl = TimeSpan.FromSeconds(settings.Value.MoviesApi.Cache.TtlInSeconds);
        }

        public override async Task<showListResponse> GetAllAsync(CancellationToken ct)
        {
            try
            {
                var grpcResponse = await base.GetAllAsync(ct);

                if (grpcResponse != null)
                {
                    await _cache.SetCacheValueAsync(ALL_MOVIES_KEY, grpcResponse, ct, _ttl);
                }

                return grpcResponse;
            }
            catch (RpcException) // transient error most likely
            {
                var cachedResponse = await _cache.GetCacheValueAsync<showListResponse>(ALL_MOVIES_KEY, ct);

                return cachedResponse;
            }
            catch (Exception)
            {
                //more serious error
                throw new UnexpectedErrorException();
            }
        }

        public override async Task<showResponse> GetByIdAsync(string id, CancellationToken ct)
        {
            try
            {
                var grpcResponse = await base.GetByIdAsync(id, ct);

                if (grpcResponse != null)
                {
                    await _cache.SetCacheValueAsync(string.Format(MOVIE_KEY, id), grpcResponse, ct, _ttl);
                }

                return grpcResponse;
            }
            catch (RpcException) // transient error most likely
            {
                var cachedResponse = await _cache.GetCacheValueAsync<showResponse>(string.Format(MOVIE_KEY, id), ct);

                return cachedResponse;
            }
            catch (Exception)
            {
                //more serious error
                throw new UnexpectedErrorException();
            }
        }
    }
}
