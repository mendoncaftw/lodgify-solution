using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Responses;
using ApiApplication.Services.Abstractions;
using ApiApplication.TypedExceptions;

namespace ApiApplication.Services
{
    public class SeatsService : ISeatsService
    {
        private readonly IShowtimesRepository _showtimesRepo;
        private readonly IAuditoriumsRepository _auditoriumsRepo;
        private readonly ITicketsRepository _ticketsRepo;

        public SeatsService(IShowtimesRepository showtimesRepository,
                            IAuditoriumsRepository auditoriumsRepository,
                            ITicketsRepository ticketsRepository)
        {
            _showtimesRepo = showtimesRepository;
            _auditoriumsRepo = auditoriumsRepository;
            _ticketsRepo = ticketsRepository;
        }

        public async Task<ReserveSeatsResponse> ReserveAsync(int showtimeId, int numberOfSeats, CancellationToken ct)
        {
            var showtime = await _showtimesRepo.GetWithMoviesByIdAsync(showtimeId, ct);

            if (showtime is null)
            {
                throw new ShowtimeNotFoundException(showtimeId);
            }

            var auditorium = await _auditoriumsRepo.GetAsync(showtime.AuditoriumId, ct);

            var showtimeSeats = await _ticketsRepo.GetEnrichedAsync(showtimeId, ct);

            var availableContiguousSeats = GetContiguousAvailableSeats(showtimeSeats, numberOfSeats, auditorium.Seats);

            if (!availableContiguousSeats.Any())
            {
                throw new SeatsNotAvailableException(numberOfSeats, showtimeId);
            }

            var reservedTicket = await _ticketsRepo.CreateAsync(showtime, availableContiguousSeats, ct);

            return new ReserveSeatsResponse
            {
                AuditoriumId = auditorium.Id,
                MovieId = reservedTicket.ShowtimeId,
                MovieTitle = showtime.Movie.Title,
                NumberOfSeats = reservedTicket.Seats.Count,
                ReservationId = reservedTicket.Id
            };
        }

        public async Task ConfirmReservationAsync(Guid reservationId, CancellationToken ct)
        {
            var reservation = await _ticketsRepo.GetAsync(reservationId, ct);

            if (reservation is null)
            {
                throw new ReservationNotFoundException(reservationId);
            }

            if (reservation.CreatedTime < DateTime.UtcNow.AddMinutes(-10))
            {
                throw new ReservationExpiredException(reservationId);
            }

            if (reservation.Paid)
            {
                throw new ReservationAlreadyConfirmedException(reservationId);
            }

            _ = await _ticketsRepo.ConfirmPaymentAsync(reservation, ct);
        }

        private IEnumerable<SeatEntity> GetContiguousAvailableSeats(IEnumerable<TicketEntity> showtimeSeats,
                                                                    int numberOfSeats,
                                                                    IEnumerable<SeatEntity> auditoriumSeats)
        {

            var occupiedSeats = showtimeSeats
                .Where(t => t.Paid || t.CreatedTime > DateTime.UtcNow.AddMinutes(-10)) // can't reserve sold seat or seat selected < 10 mins ago
                .SelectMany(s => s.Seats)
                .ToList();

            foreach (var row in auditoriumSeats.GroupBy(s => s.Row))
            {
                for (var col = 0; col <= row.Count() - numberOfSeats; col++)
                {
                    var isContiguous = true;

                    for (var s = 1; s <= numberOfSeats; s++) // Seats start at id 1
                    {
                        if (occupiedSeats.Any(seat => seat.Row == row.Key && seat.SeatNumber == (short)(col + s)))
                        {
                            isContiguous = false;

                            // jump ahead to occupied seat so the 1st for iteration can move to the next one
                            col += s;

                            break;
                        }
                    }

                    if (isContiguous)
                    {
                        return row.Skip(col).Take(numberOfSeats);
                    }
                }
            }

            return new List<SeatEntity>(0);
        }
    }
}
