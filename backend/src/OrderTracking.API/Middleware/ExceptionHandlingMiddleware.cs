using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderTracking.Domain.Exceptions;

namespace OrderTracking.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = CreateErrorResponse(context, exception);
            var statusCode = response.StatusCode;

            _logger.Log(
                GetLogLevel(statusCode),
                exception,
                "HTTP {Method} {Path} responded {StatusCode} | ErrorCode: {ErrorCode} | Message: {Message}",
                context.Request.Method,
                context.Request.Path,
                statusCode,
                response.ErrorCode,
                response.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        private static ErrorResponse CreateErrorResponse(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;

            switch (exception)
            {
                case DomainException domainEx:
                    return new ErrorResponse
                    {
                        StatusCode = domainEx.StatusCode,
                        ErrorCode = domainEx.ErrorCode,
                        Message = domainEx.Message,
                        Details = domainEx is ValidationException valEx && valEx.Errors.Count > 0
                            ? valEx.Errors
                            : null,
                        TraceId = traceId
                    };

                case InvalidOperationException invalidOpEx:
                    return new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorCode = "INVALID_OPERATION",
                        Message = invalidOpEx.Message,
                        TraceId = traceId
                    };

                case ArgumentNullException argNullEx:
                    return new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorCode = "INVALID_ARGUMENT",
                        Message = argNullEx.Message,
                        TraceId = traceId
                    };

                case KeyNotFoundException keyNotFoundEx:
                    return new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorCode = "NOT_FOUND",
                        Message = keyNotFoundEx.Message,
                        TraceId = traceId
                    };

                case NotImplementedException _:
                    return new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status501NotImplemented,
                        ErrorCode = "NOT_IMPLEMENTED",
                        Message = "The requested feature is not implemented.",
                        TraceId = traceId
                    };

                default:
                    return new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        ErrorCode = "INTERNAL_ERROR",
                        Message = "An unexpected error occurred. Please try again later.",
                        TraceId = traceId
                    };
            }
        }

        private static LogLevel GetLogLevel(int statusCode) => statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }
}