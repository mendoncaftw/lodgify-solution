using System;
using System.Collections.Generic;
using System.Text;
using ApiApplication.Controllers;
using ApiApplication.Requests;
using ApiApplication.Responses;
using ApiApplication.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using NSubstitute.ExceptionExtensions;
using ApiApplication.TypedExceptions;

namespace ApiApplication.Tests.Unit.Controllers
{
    public class SeatsControllerTest
    {
        [Fact]
        public async Task Reserve_WhenSeatsServiceSuccessful_ReturnsOkResultWithSeatsResponse()
        {
            // Arrange
            var request = new ReserveSeatsRequest { ShowtimeId = 1, NumberOfSeats = 2 };
            var cancellationToken = CancellationToken.None;
            var expectedResponse = new ReserveSeatsResponse
            {
                AuditoriumId = 1,
                MovieId = 1,
                MovieTitle = "Test",
                NumberOfSeats = request.NumberOfSeats,
                ReservationId = Guid.NewGuid()
            };

            // Mocking seats service
            var seatsService = Substitute.For<ISeatsService>();
            seatsService.ReserveAsync(request.ShowtimeId, request.NumberOfSeats, cancellationToken)
                        .Returns(Task.FromResult(expectedResponse));

            var controller = new SeatsController(seatsService);

            // Act
            var result = await controller.Reserve(request, CancellationToken.None);

            // Assert
            Assert.IsType<ActionResult<ReserveSeatsResponse>>(result);
            Assert.Equal(request.NumberOfSeats, result.Value.NumberOfSeats);
        }

        [Fact]
        public async Task Reserve_WhenShowtimeNotFoundExceptionIsThrown_ReturnsBadRequest()
        {
            // Arrange
            var request = new ReserveSeatsRequest { ShowtimeId = 1, NumberOfSeats = 2 };
            var cancellationToken = CancellationToken.None;


            // Mocking seats service
            var seatsService = Substitute.For<ISeatsService>();
            seatsService.ReserveAsync(request.ShowtimeId, request.NumberOfSeats, cancellationToken)
                        .Throws(new ShowtimeNotFoundException(request.ShowtimeId));

            var controller = new SeatsController(seatsService);

            // Act
            var result = await controller.Reserve(request, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Reserve_WhenSeatsNotAvailableExceptionIsThrown_ReturnsBadRequest()
        {
            // Arrange
            var request = new ReserveSeatsRequest { ShowtimeId = 1, NumberOfSeats = 2 };
            var cancellationToken = CancellationToken.None;


            // Mocking seats service
            var seatsService = Substitute.For<ISeatsService>();
            seatsService.ReserveAsync(request.ShowtimeId, request.NumberOfSeats, cancellationToken)
                        .Throws(new SeatsNotAvailableException(request.NumberOfSeats, request.ShowtimeId));

            var controller = new SeatsController(seatsService);

            // Act
            var result = await controller.Reserve(request, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Reserve_WhenAnyOtherExceptionIsThrown_ThrowsException()
        {
            // Arrange
            var request = new ReserveSeatsRequest { ShowtimeId = 1, NumberOfSeats = 2 };
            var cancellationToken = CancellationToken.None;


            // Mocking seats service
            var seatsService = Substitute.For<ISeatsService>();
            seatsService.ReserveAsync(request.ShowtimeId, request.NumberOfSeats, cancellationToken)
                        .Throws(new Exception());

            var controller = new SeatsController(seatsService);

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(() => controller.Reserve(request, CancellationToken.None));
        }
    }
}
