namespace ApiApplication.TypedExceptions
{
    public class ShowtimeNotFoundException : TypedException
    {
        private const string ErrorMessageFormat = "Showtime with id {0} not found";

        public ShowtimeNotFoundException(int id)
        {
            ErrorMessage = string.Format(ErrorMessageFormat, id);
        }
    }
}
