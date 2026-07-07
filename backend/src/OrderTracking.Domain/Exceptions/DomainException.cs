using System;

namespace OrderTracking.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message)
            : base(message)
        {
        }

        protected DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        public abstract int StatusCode { get; }
        public abstract string ErrorCode { get; }
    }
}