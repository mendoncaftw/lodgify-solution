namespace ApiApplication.Database.Entities
{
    public class SeatEntity
    {
        public short Row { get; set; }
        public short SeatNumber { get; set; }
        public int AuditoriumId { get; set; }

        // Navigation
        public AuditoriumEntity Auditorium { get; set; }
    }
}
