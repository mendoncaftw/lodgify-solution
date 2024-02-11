using System;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Clients.Abstractions;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Services.Abstractions;
using ApiApplication.TypedExceptions;

namespace ApiApplication.Services
{
    public class ShowtimesService : IShowtimesService
    {
        private readonly IApiClientGrpc _client;
        private readonly IShowtimesRepository _showtimesRepo;
        private readonly IAuditoriumsRepository _auditoriumsRepo;

        public ShowtimesService(IApiClientGrpc apiClientGrpc,
                                IShowtimesRepository showtimesRepository,
                                IAuditoriumsRepository auditoriumsRepository)
        {
            _client = apiClientGrpc;
            _showtimesRepo = showtimesRepository;
            _auditoriumsRepo = auditoriumsRepository;
        }

        public async Task<int> CreateAsync(string movieId, DateTime sessionDate, int auditoriumId, CancellationToken ct)
        {
            var movie = await _client.GetByIdAsync(movieId, ct);

            if (movie is null)
            {
                throw new MovieNotFoundException(movieId);
            }

            var auditorium = await _auditoriumsRepo.GetAsync(auditoriumId, ct);

            if (auditorium is null)
            {
                throw new AuditoriumNotFoundException(auditoriumId);
            }

            try
            {
                var showTime = await _showtimesRepo.CreateShowtime(new Database.Entities.ShowtimeEntity
                {
                    AuditoriumId = auditoriumId,
                    SessionDate = sessionDate,
                    Movie = new Database.Entities.MovieEntity
                    {
                        ImdbId = movie.Id,
                        ReleaseDate = new DateTime(int.Parse(movie.Year), 01, 01),
                        Title = movie.Title,
                        Stars = movie.Crew
                    }
                }, ct);

                return showTime.Id;
            }
            catch (Exception)
            {
                throw new UnexpectedErrorException();
            }
        }
    }
}
