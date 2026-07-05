using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FinGuard.UI.Common;

public static class ModelStateExtensions
{
    public static void AddServiceErrors(
        this ModelStateDictionary modelState,
        Dictionary<string, List<string>> errors,
        string? prefix = null)
    {
        foreach (var error in errors)
        {
            string modelStateKey;
            if (string.IsNullOrEmpty(error.Key))
            {
                modelStateKey = string.Empty;
            }
            else if (string.IsNullOrEmpty(prefix))
            {
                modelStateKey = error.Key;
            }
            else
            {
                modelStateKey = $"{prefix}.{error.Key}";
            }

            foreach (var message in error.Value)
            {
                modelState.AddModelError(modelStateKey, message);
            }
        }
    }
}
