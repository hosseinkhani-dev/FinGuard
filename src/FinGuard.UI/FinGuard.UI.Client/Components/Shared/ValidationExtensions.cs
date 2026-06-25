using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;

namespace FinGuard.UI.Client.Components.Shared;

public static class ValidationExtensions
{
    public static void AddApiErrors<T>(
        this ValidationMessageStore messageStore,
        EditContext editContext,
        Dictionary<string, string[]>? errors,
        T model,
        out string? globalError)
    {
        globalError = null;
        if (errors == null) return;

        foreach (var error in errors)
        {
            if (string.IsNullOrEmpty(error.Key))
            {
                globalError = error.Value.FirstOrDefault();
                continue;
            }

            // Convert json camelCase (e.g. "userName") to C# PascalCase ("UserName")
            string propertyName = char.ToUpper(error.Key[0]) + error.Key[1..];

            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
            {
                // Fallback to global error if the field can't be mapped structurally
                globalError ??= error.Value.FirstOrDefault();
                continue;
            }

            var fieldIdentifier = new FieldIdentifier(model!, property.Name);
            messageStore.Add(fieldIdentifier, error.Value);
        }

        editContext.NotifyValidationStateChanged();
    }
}