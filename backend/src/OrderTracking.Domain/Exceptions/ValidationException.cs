using System;
using System.Collections.Generic;

namespace OrderTracking.Domain.Exceptions
{
    public class ValidationException : DomainException
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string message, IDictionary<string, string[]> errors)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>(errors);
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public override int StatusCode => 400;

        public override string ErrorCode => "VALIDATION_ERROR";
    }
}