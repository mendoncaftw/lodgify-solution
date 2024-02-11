using System;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Requests;
using ApiApplication.Services.Abstractions;
using ApiApplication.TypedExceptions;
using Microsoft.AspNetCore.Mvc;

namespace ApiApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowtimesController : ControllerBase
    {
        private readonly IShowtimesService _showtimesService;

        public ShowtimesController(IShowtimesService showtimesService)
        {
            _showtimesService = showtimesService;
        }

        [HttpPost("new")]
        public async Task<ActionResult<int>> Create(CreateShowtimeRequest request, CancellationToken ct)
        {
            try
            {
                var showtimeId = await _showtimesService.CreateAsync(request.MovieId, request.SessionDate, request.AuditoriumId, ct);

                return showtimeId;
            }
            catch (Exception ex) when (ex is MovieNotFoundException || ex is AuditoriumNotFoundException || ex is UnexpectedErrorException)
            {
                return BadRequest((ex as TypedException).ErrorMessage);
            }
        }
    }
}
