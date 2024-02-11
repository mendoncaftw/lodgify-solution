using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Clients.Abstractions;
using ProtoDefinitions;
using static ProtoDefinitions.MoviesApi;

namespace ApiApplication.Clients
{
    public class ApiClientGrpc : IApiClientGrpc
    {
        private readonly MoviesApiClient _client;

        public ApiClientGrpc(MoviesApiClient client)
        {
            _client = client;
        }

        public virtual async Task<showListResponse> GetAllAsync(CancellationToken ct)
        {
            var all = await _client.GetAllAsync(new Empty(), cancellationToken: ct);
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }

        public virtual async Task<showResponse> GetByIdAsync(string id, CancellationToken ct)
        {
            var show = await _client.GetByIdAsync(new IdRequest { Id = id }, cancellationToken: ct);
            show.Data.TryUnpack<showResponse>(out var data);
            return data;
        }
    }
}