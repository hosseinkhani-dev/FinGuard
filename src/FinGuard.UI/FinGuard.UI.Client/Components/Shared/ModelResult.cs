namespace FinGuard.UI.Client.Components.Shared;

public class ModelResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ModelResult<T> Failure(string message) =>
        new() { 
            IsSuccess = false,
            Errors = new Dictionary<string,string[]> { { "", new[] { message } } } 
        };
}
