using System;

namespace ApiApplication.TypedExceptions
{
    public abstract class TypedException : Exception
    {
        public string ErrorMessage { get; set; }
    }
}
