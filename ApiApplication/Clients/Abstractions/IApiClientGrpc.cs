using System.Threading;
using System.Threading.Tasks;
using ProtoDefinitions;

namespace ApiApplication.Clients.Abstractions
{
    public interface IApiClientGrpc
    {
        Task<showListResponse> GetAllAsync(CancellationToken ct);
        Task<showResponse> GetByIdAsync(string id, CancellationToken ct);
    }
}
