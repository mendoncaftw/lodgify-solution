using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Services;
using ApiApplication.TypedExceptions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace ApiApplication.Tests.Unit.Services
{
    public class SeatsServiceTest
    {
        [Fact]
        public async Task ConfirmReservationAsync_WhenReservationIsValid_ConfirmsPayment()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            var ticketEntity = new TicketEntity { Id = reservationId, Paid = false, CreatedTime = DateTime.UtcNow };

            // Mocking the repo
            var ticketsRepo = Substitute.For<ITicketsRepository>();
            ticketsRepo.GetAsync(reservationId, cancellationToken)
                       .Returns(ticketEntity);
            ticketsRepo.ConfirmPaymentAsync(ticketEntity, cancellationToken)
                       .Returns(x =>
                       {
                           ticketEntity.Paid = true;
                           return ticketEntity;
                       });

            var service = new SeatsService(Substitute.For<IShowtimesRepository>(), Substitute.For<IAuditoriumsRepository>(), ticketsRepo);

            // Act
            await service.ConfirmReservationAsync(reservationId, cancellationToken);

            // Assert
            await ticketsRepo.Received(1).GetAsync(reservationId, cancellationToken);
            await ticketsRepo.Received(1).ConfirmPaymentAsync(ticketEntity, cancellationToken);
            Assert.True(ticketEntity.Paid);
        }

        [Fact]
        public async Task ConfirmReservationAsync_WhenReservationIsNotFound_Throws_ReservationNotFoundException()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;

            // Mocking the repo
            var ticketsRepo = Substitute.For<ITicketsRepository>();
            ticketsRepo.GetAsync(reservationId, cancellationToken)
                       .ReturnsNull();

            var service = new SeatsService(Substitute.For<IShowtimesRepository>(), Substitute.For<IAuditoriumsRepository>(), ticketsRepo);

            // Act + Assert
            await Assert.ThrowsAsync<ReservationNotFoundException>(() => service.ConfirmReservationAsync(reservationId, cancellationToken));
        }

        [Fact]
        public async Task ConfirmReservationAsync_WhenReservationIsExpired_Throws_ReservationExpiredException()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            var ticketEntity = new TicketEntity { Id = reservationId, Paid = false, CreatedTime = DateTime.UtcNow.AddMinutes(-11) };

            // Mocking the repo
            var ticketsRepo = Substitute.For<ITicketsRepository>();
            ticketsRepo.GetAsync(reservationId, cancellationToken)
                       .Returns(ticketEntity);

            var service = new SeatsService(Substitute.For<IShowtimesRepository>(), Substitute.For<IAuditoriumsRepository>(), ticketsRepo);

            // Act + Assert
            await Assert.ThrowsAsync<ReservationExpiredException>(() => service.ConfirmReservationAsync(reservationId, cancellationToken));
        }

        [Fact]
        public async Task ConfirmReservationAsync_WhenReservationIsAlreadyConfirmed_Throws_ReservationAlreadyConfirmedException()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            var ticketEntity = new TicketEntity { Id = reservationId, Paid = true, CreatedTime = DateTime.UtcNow };

            // Mocking the repo
            var ticketsRepo = Substitute.For<ITicketsRepository>();
            ticketsRepo.GetAsync(reservationId, cancellationToken)
                       .Returns(ticketEntity);

            var service = new SeatsService(Substitute.For<IShowtimesRepository>(), Substitute.For<IAuditoriumsRepository>(), ticketsRepo);

            // Act + Assert
            await Assert.ThrowsAsync<ReservationAlreadyConfirmedException>(() => service.ConfirmReservationAsync(reservationId, cancellationToken));
        }

        [Fact]
        public async Task ReserveAsync_WhenShowtimeIsNotFound_Throws_ShowtimeNotFoundException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var showtimeId = 1;
            var numberOfSeats = 2;

            // Mocking showtimes repo
            var showtimesRepo = Substitute.For<IShowtimesRepository>();
            showtimesRepo.GetWithMoviesByIdAsync(Arg.Any<int>(), cancellationToken)
                         .ReturnsNull();

            var service = new SeatsService(showtimesRepo, Substitute.For<IAuditoriumsRepository>(), Substitute.For<ITicketsRepository>());

            // Act + Assert
            await Assert.ThrowsAsync<ShowtimeNotFoundException>(() => service.ReserveAsync(showtimeId, numberOfSeats, cancellationToken));
        }

        [Theory]
        [MemberData(nameof(NoAvailableContiguousSeatsWithPaidSeats))]
        [MemberData(nameof(NoAvailableContiguousSeatsWithPaidAndPendingSeats))]
        [MemberData(nameof(NoAvailableContiguousSeatsShortRows1))]
        [MemberData(nameof(NoAvailableContiguousSeatsShortRows2))]
        public async Task ReserveAsync_WhenNoAvailableContiguousSeats_Throws_SeatsNotAvailableException(List<SeatEntity> auditoriumSeats, List<TicketEntity> tickets, int reservationSeats)
        {
            // Arrange
            var showtimeId = 1;
            var auditoriumId = 1;
            var cancellationToken = CancellationToken.None;
            var showtime = new ShowtimeEntity { Id = showtimeId, AuditoriumId = auditoriumId, Movie = new MovieEntity { Title = "Test Movie" } };
            var auditorium = new AuditoriumEntity
            {
                Id = auditoriumId,
                Seats = auditoriumSeats
            };

            // Mocking showtimes repo
            var showtimesRepo = Substitute.For<IShowtimesRepository>();
            showtimesRepo.GetWithMoviesByIdAsync(showtimeId, cancellationToken)
                         .Returns(Task.FromResult(showtime));

            // Mocking auditoriums repo
            var auditoriumsRepo = Substitute.For<IAuditoriumsRepository>();
            auditoriumsRepo.GetAsync(showtime.AuditoriumId, cancellationToken)
                           .Returns(Task.FromResult(auditorium));

            // Mocking tickets repo
            var ticketsRepo = Substitute.For<ITicketsRepository>();
            ticketsRepo.GetEnrichedAsync(showtimeId, cancellationToken)
                       .Returns(tickets);

            var service = new SeatsService(showtimesRepo, auditoriumsRepo, ticketsRepo);

            // Act + Assert
            await Assert.ThrowsAsync<SeatsNotAvailableException>(() => service.ReserveAsync(showtimeId, reservationSeats, cancellationToken));

            await showtimesRepo.Received(1).GetWithMoviesByIdAsync(showtimeId, cancellationToken);
            await auditoriumsRepo.Received(1).GetAsync(auditoriumId, cancellationToken);
            await ticketsRepo.Received(1).GetEnrichedAsync(showtimeId, cancellationToken);
        }

        [Theory]
        [MemberData(nameof(AvailableContiguousSeatsWithPaidSeats))]
        [MemberData(nameof(AvailableContiguousSeatsWithPaidAndPendingSeats))]
        [MemberData(nameof(AvailableContiguousSeatsWithPaidAndPendingAndExpiredSeats))]
        [MemberData(nameof(AvailableContiguousSeatsWithNoOccupiedSeats))]
        public async Task ReserveAsync_WhenAvailableContiguousSeats_Returns_ReserveSeatsResponse(List<SeatEntity> auditoriumSeats, List<TicketEntity> tickets, int reservationSeats)
        {
            // Arrange
            var showtimeId = 1;
            var auditoriumId = 1;
            var cancellationToken = CancellationToken.None;
            var showtime = new ShowtimeEntity { Id = showtimeId, AuditoriumId = auditoriumId, Movie = new MovieEntity { Title = "Test Movie" } };
            var auditorium = new AuditoriumEntity
            {
                Id = auditoriumId,
                Seats = auditoriumSeats
            };
            var expectedResult = new TicketEntity
            {
                ShowtimeId = showtimeId,
                Seats = CreateNumberOfSeats(reservationSeats),
                Id = Guid.NewGuid()
            };

            // Mocking showtimes repo
            var showtimesRepo = Substitute.For<IShowtimesRepository>();
            showtimesRepo.GetWithMoviesByIdAsync(showtimeId, cancellationToken)
                         .Returns(Task.FromResult(showtime));

            // Mocking auditoriums repo
            var auditoriumsRepo = Substitute.For<IAuditoriumsRepository>();
            auditoriumsRepo.GetAsync(showtime.AuditoriumId, cancellationToken)
                           .Returns(Task.FromResult(auditorium));

            // Mocking tickets repo
            var ticketsRepo = Substitute.For<ITicketsRepository>();
            ticketsRepo.GetEnrichedAsync(showtimeId, cancellationToken)
                       .Returns(tickets);
            ticketsRepo.CreateAsync(showtime, Arg.Any<IEnumerable<SeatEntity>>(), cancellationToken)
                       .Returns(expectedResult);

            var service = new SeatsService(showtimesRepo, auditoriumsRepo, ticketsRepo);

            // Act
            var response = await service.ReserveAsync(showtimeId, reservationSeats, cancellationToken);

            // Assert
            await showtimesRepo.Received(1).GetWithMoviesByIdAsync(showtimeId, cancellationToken);
            await auditoriumsRepo.Received(1).GetAsync(auditoriumId, cancellationToken);
            await ticketsRepo.Received(1).GetEnrichedAsync(showtimeId, cancellationToken);
            Assert.Equal(reservationSeats, response.NumberOfSeats);
        }

        // Available Contiguous Seats
        public static IEnumerable<object[]> AvailableContiguousSeatsWithPaidSeats()
        {
            yield return GenerateScenario(2, 10, new List<TicketEntity>
            {
                Stubs.CreatePaidTicket(1, 5, 1), 
                Stubs.CreatePaidTicket(2, 1, 5)
            }, 3);
        }

        public static IEnumerable<object[]> AvailableContiguousSeatsWithPaidAndPendingSeats()
        {
            yield return GenerateScenario(3, 10, new List<TicketEntity>
            {
                    Stubs.CreatePaidTicket(1, 1, 10),
                    Stubs.CreatePendingTicket(2, 3, 3),
                    Stubs.CreatePendingTicket(2, 7, 3),
                    Stubs.CreatePaidTicket(3, 1, 6),
                    Stubs.CreatePaidTicket(3, 8, 2),
            }, 2);
        }

        public static IEnumerable<object[]> AvailableContiguousSeatsWithPaidAndPendingAndExpiredSeats()
        {
            yield return GenerateScenario(3, 10, new List<TicketEntity>
            {
                    Stubs.CreateExpiredTicket(1, 1, 10),
                    Stubs.CreatePendingTicket(2, 3, 3),
                    Stubs.CreatePendingTicket(2, 7, 3),
                    Stubs.CreatePaidTicket(3, 1, 6),
                    Stubs.CreatePaidTicket(3, 8, 2),
            }, 10);
        }

        public static IEnumerable<object[]> AvailableContiguousSeatsWithNoOccupiedSeats()
        {
            yield return GenerateScenario(3, 10, new List<TicketEntity>(0), 10);
        }

        // No Available Contiguous Seats
        public static IEnumerable<object[]> NoAvailableContiguousSeatsShortRows1()
        {
            yield return GenerateScenario(2, 10, new List<TicketEntity>(0), 11);
        }

        public static IEnumerable<object[]> NoAvailableContiguousSeatsShortRows2()
        {
            yield return GenerateScenario(3, 20, new List<TicketEntity>(0), 100);
        }

        public static IEnumerable<object[]> NoAvailableContiguousSeatsWithPaidSeats()
        {
            // Auditorium 2 rows 10 seats per row
            yield return GenerateScenario(2, 10, new List<TicketEntity>
            {
                Stubs.CreatePaidTicket(1, 5, 1), // 1st row 5th seat 1 occupied
                Stubs.CreatePaidTicket(2, 1, 5) // 2nd row 1st seat 5 occupied
            }, 6); // try to place 6 seats
        }

        public static IEnumerable<object[]> NoAvailableContiguousSeatsWithPaidAndPendingSeats()
        {
            yield return GenerateScenario(3, 10, new List<TicketEntity>
            {
                    Stubs.CreatePaidTicket(1, 4, 6),
                    Stubs.CreatePendingTicket(2, 3, 3),
                    Stubs.CreatePendingTicket(2, 7, 3),
                    Stubs.CreatePaidTicket(3, 1, 6),
                    Stubs.CreatePaidTicket(3, 8, 2),
            }, 4);
        }

        public static object[] GenerateScenario(int nrRows, int nrColumns, List<TicketEntity> occupation, int reservations)
        {
            var auditorium = GenerateAuditoriumSeats(nrRows, nrColumns);

            return new object[] { auditorium, occupation, reservations };
        }

        private static List<SeatEntity> CreateNumberOfSeats(int num)
        {
            var seats = new List<SeatEntity>();

            for (int i = 0; i < num; i++)
            {
                seats.Add(new SeatEntity());
            }

            return seats;
        }

        private static List<SeatEntity> GenerateAuditoriumSeats(int rows, int seatsPerRow)
        {
            var seats = new List<SeatEntity>();
            for (short r = 1; r <= rows; r++)
                for (short s = 1; s <= seatsPerRow; s++)
                    seats.Add(new SeatEntity { Row = r, SeatNumber = s });

            return seats;
        }

        private static List<SeatEntity> CreateRowSeats(int numberOfSeats)
        {
            var seats = new List<SeatEntity>();

            for (int i = 0; i < numberOfSeats; i++)
            {
                seats.Add(new SeatEntity());
            }

            return seats;
        }
    }

    public static class Stubs
    {
        private static List<SeatEntity> GenerateSeats(int row, int column, int numberOfSeats)
        {
            var seats = new List<SeatEntity>();

            for (int i = 0; i < numberOfSeats; i++)
            {
                seats.Add(new SeatEntity
                {
                    Row = (short)row,
                    SeatNumber = (short)(column + i)
                });
            }

            return seats;
        }

        public static TicketEntity CreatePaidTicket(int row, int column, int numberOfSeats)
        {
            return new TicketEntity
            {
                Paid = true,
                CreatedTime = DateTime.UtcNow,
                Seats = GenerateSeats(row, column, numberOfSeats)
            };
        }

        public static TicketEntity CreateExpiredTicket(int row, int column, int numberOfSeats)
        {
            return new TicketEntity
            {
                Paid = false,
                CreatedTime = DateTime.UtcNow.AddDays(-1),
                Seats = GenerateSeats(row, column, numberOfSeats)
            };
        }

        public static TicketEntity CreatePendingTicket(int row, int column, int numberOfSeats)
        {
            return new TicketEntity
            {
                Paid = false,
                CreatedTime = DateTime.UtcNow,
                Seats = GenerateSeats(row, column, numberOfSeats)
            };
        }
    }
}

