namespace ApiApplication.Requests
{
    public class ReserveSeatsRequest
    {
        public int ShowtimeId { get; set; }
        public int NumberOfSeats { get; set; }
    }
}
