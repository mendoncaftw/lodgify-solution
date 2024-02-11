using System;

namespace ApiApplication.TypedExceptions
{
    public class ReservationAlreadyConfirmedException : TypedException
    {
        private const string ErrorMessageFormat = "Reservation with id {0} has already been confirmed";

        public ReservationAlreadyConfirmedException(Guid id)
        {
            ErrorMessage = string.Format(ErrorMessageFormat, id);
        }
    }
}
