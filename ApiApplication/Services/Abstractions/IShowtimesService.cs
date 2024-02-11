using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Services.Abstractions
{
    public interface IShowtimesService
    {
        Task<int> CreateAsync(string movieId, DateTime sessionDate, int auditoriumId, CancellationToken ct);
    }
}
