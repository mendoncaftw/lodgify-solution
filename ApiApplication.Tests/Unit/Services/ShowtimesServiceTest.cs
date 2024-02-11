using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using NSubstitute;
using System.Threading.Tasks;
using System.Threading;
using System;
using Xunit;
using ProtoDefinitions;
using ApiApplication.Clients.Abstractions;
using ApiApplication.Services;
using ApiApplication.TypedExceptions;
using NSubstitute.ExceptionExtensions;

namespace ApiApplication.Tests.Unit.Services
{
    public class ShowtimesServiceTest
    {
        [Fact]
        public async Task CreateAsync_WithValidData_ShouldReturnShowTimeId()
        {
            // Arrange
            var movieId = "1";
            var showtimeId = 1;
            var cancellationToken = CancellationToken.None;
            var sessionDate = DateTime.Now;
            var auditoriumId = 1;
            var movie = new showResponse
            {
                Id = movieId,
                Year = "2022",
                Title = "Test Movie",
                Crew = "Test Crew"
            };

            var auditorium = new AuditoriumEntity
            {
                Id = 1,
            };

            // Mocking the api client
            var apiClient = Substitute.For<IApiClientGrpc>();
            apiClient.GetByIdAsync(movieId, cancellationToken)
                     .Returns(Task.FromResult(movie));


            var auditoriumsRepo = Substitute.For<IAuditoriumsRepository>();
            auditoriumsRepo.GetAsync(auditoriumId, cancellationToken)
                           .Returns(auditorium);

            // Mocking the repo
            var showtimesRepo = Substitute.For<IShowtimesRepository>();
            showtimesRepo.CreateShowtime(Arg.Any<ShowtimeEntity>(), cancellationToken)
                         .Returns(new ShowtimeEntity { Id = showtimeId });

            var service = new ShowtimesService(apiClient, showtimesRepo, auditoriumsRepo);

            // Act
            var result = await service.CreateAsync(movieId, sessionDate, auditoriumId, cancellationToken);

            // Assert
            Assert.Equal(showtimeId, result);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidMovie_ShouldThrow_MovieNotFoundException()
        {
            // Arrange
            var movieId = "1";
            var sessionDate = DateTime.Now;
            var auditoriumId = 1;
            var cancellationToken = CancellationToken.None;
            var movie = new showResponse
            {
                Id = movieId,
                Year = "2022",
                Title = "Test Movie",
                Crew = "Test Crew"
            };

            // Mocking the api client
            var client = Substitute.For<IApiClientGrpc>();
            client.GetByIdAsync(movieId, cancellationToken)
                  .Returns(Task.FromResult<showResponse>(null));

            // Act
            var service = new ShowtimesService(client, Substitute.For<IShowtimesRepository>(), Substitute.For<IAuditoriumsRepository>());

            // Act and Assert
            var exception = await Assert.ThrowsAsync<MovieNotFoundException>(() => service.CreateAsync(movieId, sessionDate, auditoriumId, cancellationToken));
            Assert.Contains(movieId, exception.ErrorMessage);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidAuditorium_ShouldThrow_AuditoriumNotFoundException()
        {
            // Arrange
            var movieId = "1";
            var sessionDate = DateTime.Now;
            var auditoriumId = 123;
            var cancellationToken = CancellationToken.None;
            var movie = new showResponse
            {
                Id = movieId,
                Year = "2022",
                Title = "Test Movie",
                Crew = "Test Crew"
            };

            // Mocking the api client
            var client = Substitute.For<IApiClientGrpc>();
            client.GetByIdAsync(movieId, cancellationToken)
                  .Returns(Task.FromResult(movie));

            // Mocking the repo
            var auditoriumsRepo = Substitute.For<IAuditoriumsRepository>();
            auditoriumsRepo.GetAsync(auditoriumId, cancellationToken)
                           .Returns(Task.FromResult<AuditoriumEntity>(null));

            var service = new ShowtimesService(client, Substitute.For<IShowtimesRepository>(), auditoriumsRepo);

            // Act and Assert
            var exception = await Assert.ThrowsAsync<AuditoriumNotFoundException>(() => service.CreateAsync(movieId, sessionDate, auditoriumId, cancellationToken));
            Assert.Contains(auditoriumId.ToString(), exception.ErrorMessage);
        }

        [Fact]
        public async Task CreateAsync_WithExceptionInShowtimeCreation_ShouldThrow_UnexpectedErrorException()
        {
            // Arrange
            var movieId = "1";
            var sessionDate = DateTime.Now;
            var auditoriumId = 123;
            var cancellationToken = CancellationToken.None;
            var movie = new showResponse
            {
                Id = movieId,
                Year = "2022",
                Title = "Test Movie",
                Crew = "Test Crew"
            };

            var auditorium = new AuditoriumEntity
            {
                Id = 1,
            };

            // Mocking the client
            var client = Substitute.For<IApiClientGrpc>();
            client.GetByIdAsync(movieId, cancellationToken)
                  .Returns(Task.FromResult(movie));

            // Mocking the repo
            var auditoriumsRepo = Substitute.For<IAuditoriumsRepository>();
            auditoriumsRepo.GetAsync(auditoriumId, cancellationToken)
                           .Returns(Task.FromResult(auditorium));

            // Mocking the repo
            var showtimesRepo = Substitute.For<IShowtimesRepository>();
            showtimesRepo.CreateShowtime(Arg.Any<ShowtimeEntity>(), cancellationToken)
                         .Throws(new Exception("Database error"));

            var service = new ShowtimesService(client, showtimesRepo, auditoriumsRepo);

            // Act and Assert
            var exception = await Assert.ThrowsAsync<UnexpectedErrorException>(() => service.CreateAsync(movieId, sessionDate, auditoriumId, cancellationToken));
            Assert.Equal("Unexpected error", exception.ErrorMessage);
        }
    }
}
