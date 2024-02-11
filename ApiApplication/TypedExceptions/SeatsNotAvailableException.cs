namespace ApiApplication.TypedExceptions
{
    public class SeatsNotAvailableException : TypedException
    {
        private const string ErrorMessageFormat = "Could not find {0} seats available for showtime with id {0}";

        public SeatsNotAvailableException(int numberOfSeats, int showtimeId)
        {
            ErrorMessage = string.Format(ErrorMessageFormat, numberOfSeats, showtimeId);
        }
    }
}
