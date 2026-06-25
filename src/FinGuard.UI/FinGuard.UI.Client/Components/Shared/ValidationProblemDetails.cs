namespace FinGuard.UI.Client.Components.Shared;

public class ValidationProblemDetails
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();
}
