using FinGuard.Application.Commons.Exceptions;
using FinGuard.Domain.Exceptions;
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
            var validationProblem = new HttpValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Detail = "Please correct the errors and try again."
            };

            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            foreach (var error in errors)
            {
                validationProblem.Errors.Add(error.Key, error.Value);
            }

            await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
            return true;
        }

        var (statusCode, title, type) = exception switch
        {
            DomainException => (
                StatusCodes.Status400BadRequest,
                "A domain validation error occurred.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"),

            ImportValidationException => (
            StatusCodes.Status400BadRequest,
            "An Imported row is not valid",
            "https://tools.ietf.org/html/rfc7231#section-6.5.1"),

            ConflictException => (
                StatusCodes.Status409Conflict,
                "A conflict occurred with the current state of the resource.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.9"),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "The requested resource was not found.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.4"),

            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized access.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.5"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred on the server.");
        }
        else
        {
            _logger.LogWarning("Application warning occurred: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = type,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected server error occurred. Please contact system support."
            : exception.Message
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
