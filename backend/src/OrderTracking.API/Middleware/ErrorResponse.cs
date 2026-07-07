using System;

namespace OrderTracking.API.Middleware
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; } = "INTERNAL_ERROR";
        public string Message { get; set; } = "An unexpected error occurred.";
        public object? Details { get; set; }
        public string? TraceId { get; set; }
    }
}