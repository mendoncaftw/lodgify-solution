using System;

namespace ApiApplication.TypedExceptions
{
    public class ReservationNotFoundException : TypedException
    {
        private const string ErrorMessageFormat = "Reservation with id {0} has expired";

        public ReservationNotFoundException(Guid id)
        {
            ErrorMessage = string.Format(ErrorMessageFormat, id.ToString());
        }
    }
}
