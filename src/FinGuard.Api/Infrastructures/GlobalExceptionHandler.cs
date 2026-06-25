using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

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

        // If it's some other unexpected crash, let it fall through (or handle 500s here too)
        return false;
    }
}
