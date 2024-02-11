using System;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Responses;

namespace ApiApplication.Services.Abstractions
{
    public interface ISeatsService
    {
        Task<ReserveSeatsResponse> ReserveAsync(int showtimeId, int numberOfSeats, CancellationToken ct);
        Task ConfirmReservationAsync(Guid reservationId, CancellationToken ct);
    }
}
