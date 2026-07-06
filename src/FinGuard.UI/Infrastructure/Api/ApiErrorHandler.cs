using Microsoft.AspNetCore.Mvc;

namespace FinGuard.UI.Infrastructure.Api;

public static class ApiErrorHandler
{
    public static async Task<Dictionary<string, List<string>>> ParseErrorsAsync(HttpResponseMessage response)
    {
        var errors = new Dictionary<string, List<string>>();

        try
        {
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors != null)
                {
                    return validationDetails.Errors.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToList()
                    );
                }
            }

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            var errorMessage = problemDetails?.Detail ?? problemDetails?.Title ?? "An unexpected error occurred.";
            errors.Add(string.Empty, new List<string> { errorMessage });
        }
        catch
        {
            errors.Add(string.Empty, new List<string> { $"Server error status code: {response.StatusCode}" });
        }

        return errors;
    }
}
