using System;
using System.Collections.Generic;

namespace ApiApplication.Database.Entities
{
    public class ShowtimeEntity
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }

        // Navigation
        public MovieEntity Movie { get; set; }
        public ICollection<TicketEntity> Tickets { get; set; }
    }
}
