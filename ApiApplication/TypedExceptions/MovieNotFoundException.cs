namespace ApiApplication.TypedExceptions
{
    public class MovieNotFoundException : TypedException
    {
        private const string ErrorMessageFormat = "Movie with id {0} not found";

        public MovieNotFoundException(string id)
        {
            ErrorMessage = string.Format(ErrorMessageFormat, id);
        }
    }
}
