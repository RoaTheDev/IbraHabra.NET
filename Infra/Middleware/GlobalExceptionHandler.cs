using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IbraHabra.NET.Infra.Middleware;

internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, clientMessage, logAsError) = exception switch
        {
            TimeoutException => (StatusCodes.Status504GatewayTimeout, "Service temporarily unavailable.", false),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Resource conflict.", false),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication required.", false),
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message, false),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request.", false),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed.", false),
            _ => (StatusCodes.Status500InternalServerError, "An un-handled error occurred.", true)
        };

        var correlationId = httpContext.TraceIdentifier;


        ProblemDetails problemDetails = new ProblemDetails
        {
            Type = GetErrorType(statusCode),
            Title = logAsError ? "An unexpected error occurred" : "An error occurred",
            Detail = clientMessage,
            Status = statusCode
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
    }

    private static string GetErrorType(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        504 => "Gateway Timeout",
        _ => "Unknown Error"
    };
}