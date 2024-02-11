using System;

namespace ApiApplication.Requests
{
    public class CreateShowtimeRequest
    {
        public string MovieId { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }
}
