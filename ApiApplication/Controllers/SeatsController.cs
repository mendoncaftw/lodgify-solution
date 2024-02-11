using System;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Requests;
using ApiApplication.Responses;
using ApiApplication.Services.Abstractions;
using ApiApplication.TypedExceptions;
using Microsoft.AspNetCore.Mvc;

namespace ApiApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatsService _seatsService;

        public SeatsController(ISeatsService seatsService)
        {
            _seatsService = seatsService;
        }

        [HttpPost("reserve")]
        public async Task<ActionResult<ReserveSeatsResponse>> Reserve(ReserveSeatsRequest request, CancellationToken ct)
        {
            try
            {
                var seatsResponse = await _seatsService.ReserveAsync(request.ShowtimeId, request.NumberOfSeats, ct);

                return seatsResponse;
            }
            catch (Exception ex) when (ex is ShowtimeNotFoundException || ex is SeatsNotAvailableException)
            {
                return BadRequest((ex as TypedException).ErrorMessage);
            }
        }

        [HttpPost("confirm-reservation")]
        public async Task<ActionResult> ConfirmReservation(Guid reservationId, CancellationToken ct)
        {
            try
            {
                await _seatsService.ConfirmReservationAsync(reservationId, ct);
            }
            catch (Exception ex) when (ex is Exception && (ex.Message.Contains("expired") || ex.Message.Contains("confirmed")))
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
