using ApiApplication.Controllers;
using ApiApplication.Requests;
using ApiApplication.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Threading.Tasks;
using System.Threading;
using System;
using Xunit;
using ApiApplication.TypedExceptions;
using NSubstitute.ExceptionExtensions;

namespace ApiApplication.Tests.Unit.Controllers
{
    public class ShowtimesControllerTest
    {
        [Fact]
        public async Task Create_WhenShowtimeIsValid_ReturnsId()
        {
            // Arrange
            var request = new CreateShowtimeRequest { MovieId = "1", SessionDate = DateTime.Now, AuditoriumId = 1 };
            var cancellationToken = CancellationToken.None;
            var expectedId = 1;

            // Mocking the showtimes service
            var showtimesService = Substitute.For<IShowtimesService>();
            showtimesService.CreateAsync(request.MovieId, request.SessionDate, request.AuditoriumId, cancellationToken)
                            .Returns(Task.FromResult(expectedId));

            var controller = new ShowtimesController(showtimesService);

            // Act
            var result = await controller.Create(request, cancellationToken);

            // Assert
            Assert.IsType<ActionResult<int>>(result);
            Assert.Equal(expectedId, result.Value);
        }

        [Fact]
        public async Task Create_WhenMovieNotFoundExceptionIsThrown_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateShowtimeRequest { MovieId = "1", SessionDate = DateTime.Now, AuditoriumId = 1 };
            var cancellationToken = CancellationToken.None;

            // Mocking the showtimes service
            var showtimesService = Substitute.For<IShowtimesService>();
            showtimesService.CreateAsync(request.MovieId, request.SessionDate, request.AuditoriumId, cancellationToken)
                            .Throws(new MovieNotFoundException(request.MovieId));

            var controller = new ShowtimesController(showtimesService);

            // Act
            var result = await controller.Create(request, cancellationToken);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_WhenAuditoriumNotFoundExceptionIsThrown_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateShowtimeRequest { MovieId = "1", SessionDate = DateTime.Now, AuditoriumId = 1 };
            var cancellationToken = CancellationToken.None;

            // Mocking the showtimes service
            var showtimesService = Substitute.For<IShowtimesService>();
            showtimesService.CreateAsync(request.MovieId, request.SessionDate, request.AuditoriumId, cancellationToken)
                            .Throws(new AuditoriumNotFoundException(request.AuditoriumId));

            var controller = new ShowtimesController(showtimesService);

            // Act
            var result = await controller.Create(request, cancellationToken);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_WhenUnexpectedErrorExceptionIsThrown_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateShowtimeRequest { MovieId = "1", SessionDate = DateTime.Now, AuditoriumId = 1 };
            var cancellationToken = CancellationToken.None;

            // Mocking the showtimes service
            var showtimesService = Substitute.For<IShowtimesService>();
            showtimesService.CreateAsync(request.MovieId, request.SessionDate, request.AuditoriumId, cancellationToken)
                            .Throws(new UnexpectedErrorException());

            var controller = new ShowtimesController(showtimesService);

            // Act
            var result = await controller.Create(request, cancellationToken);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_WhenAnyOtherExceptionIsThrown_ThrowsException()
        {
            // Arrange
            var request = new CreateShowtimeRequest { MovieId = "1", SessionDate = DateTime.Now, AuditoriumId = 1 };
            var cancellationToken = CancellationToken.None;

            // Mocking the showtimes service
            var showtimesService = Substitute.For<IShowtimesService>();
            showtimesService.CreateAsync(request.MovieId, request.SessionDate, request.AuditoriumId, cancellationToken)
                            .Throws(new Exception());

            var controller = new ShowtimesController(showtimesService);

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() => controller.Create(request, cancellationToken));
        }
    }
}
