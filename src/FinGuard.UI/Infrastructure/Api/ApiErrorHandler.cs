using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace FinGuard.UI.Infrastructure.Api;

public static class ApiErrorHandler
{
    public static async Task<Dictionary<string, List<string>>> ParseErrorsAsync(HttpResponseMessage response)
    {
        var errors = new Dictionary<string, List<string>>();

        try
        {
            var json = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var validationDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(json);

                if (validationDetails?.Errors?.Count > 0)
                {
                    return validationDetails.Errors.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToList());
                }
            }

            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(json);

            var errorMessage = problemDetails?.Detail
                ?? problemDetails?.Title
                ?? "An unexpected error occurred.";

            errors.Add(string.Empty, new List<string> { errorMessage });
        }
        catch
        {
            errors.Add(string.Empty,
                new List<string> { $"Server error status code: {(int)response.StatusCode}" });
        }

        return errors;
    }
}
