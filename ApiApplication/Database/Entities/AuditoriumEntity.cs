using System.Collections.Generic;

namespace ApiApplication.Database.Entities
{
    public class AuditoriumEntity
    {
        public int Id { get; set; }

        // Navigation
        public List<ShowtimeEntity> Showtimes { get; set; }
        public ICollection<SeatEntity> Seats { get; set; }
       
    }
}
