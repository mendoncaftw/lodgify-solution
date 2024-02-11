using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Database.Entities;

namespace ApiApplication.Database.Repositories.Abstractions
{
    public interface IAuditoriumsRepository
    {
        Task<AuditoriumEntity> GetAsync(int auditoriumId, CancellationToken cancel);
    }
}