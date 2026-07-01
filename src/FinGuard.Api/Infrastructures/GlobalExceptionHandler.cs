using FinGuard.Application.Commons.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinGuard.Api.Infrastructures;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Check if the exception is our FluentValidation exception
        if (exception is ValidationException validationException)
        {
            _logger.LogWarning("Validation failed for request: {Message}", exception.Message);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            // 2. Format the errors into a standard RFC 7807 Problem Details object
            var problemDetails = new HttpValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Detail = "Please correct the errors and try again."
            };

            // 3. Group the errors by property name so the frontend gets a clean dictionary
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            foreach (var error in errors)
            {
                problemDetails.Errors.Add(error.Key, error.Value);
            }

            // 4. Write the JSON response back to the client
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        // Business Conflicts (HTTP 409 Conflict)
        if (exception is ConflictException conflictException)
        {
            _logger.LogWarning("Business conflict occurred: {Message}", exception.Message);

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                Title = "A conflict occurred with the current state of the resource.",
                Detail = conflictException.Message
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        // HTTP 404 Not Found
        if (exception is NotFoundException notFoundException)
        {
            _logger.LogWarning("Resource not found: {Message}", exception.Message);

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "The requested resource was not found.",
                Detail = notFoundException.Message
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        // HTTP 401 Unauthorized
        if (exception is UnauthorizedException unauthorizedException)
        {
            _logger.LogWarning("Resource not found: {Message}", exception.Message);

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Unauthorized.",
                Detail = unauthorizedException.Message
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        // HTTP 500 Error
        _logger.LogError(exception, "An unhandled exception occurred on the server.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var internalErrorDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected server error occurred. Please contact system support."
        };

        await httpContext.Response.WriteAsJsonAsync(internalErrorDetails, cancellationToken);
        return true;
    }
}
