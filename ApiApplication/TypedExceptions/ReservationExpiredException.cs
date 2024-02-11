using System;

namespace ApiApplication.TypedExceptions
{
    public class ReservationExpiredException : TypedException
    {
        private const string ErrorMessageFormat = "Reservation with id {0} has expired";

        public ReservationExpiredException(Guid id)
        {
            ErrorMessage = string.Format(ErrorMessageFormat, id);
        }
    }
}
