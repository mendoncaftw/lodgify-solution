namespace ApiApplication.TypedExceptions
{
    public class UnexpectedErrorException : TypedException
    {
        private const string ErrorMessageFormat = "Unexpected error";

        public UnexpectedErrorException()
        {
            ErrorMessage = ErrorMessageFormat;
        }
    }
}
