namespace ApiApplication.TypedExceptions
{
    public class AuditoriumNotFoundException : TypedException
    {
        private const string ErrorMessageFormat = "Auditorium with id {0} not found";

        public AuditoriumNotFoundException(int id)
        {
            ErrorMessage = string.Format(ErrorMessageFormat, id);
        }
    }
}
