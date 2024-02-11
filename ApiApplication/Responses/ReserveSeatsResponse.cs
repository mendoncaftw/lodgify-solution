using System;

namespace ApiApplication.Responses
{
    public class ReserveSeatsResponse
    {
        public Guid ReservationId { get; set; }
        public int NumberOfSeats { get; set; }

        public int AuditoriumId { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
    }
}
