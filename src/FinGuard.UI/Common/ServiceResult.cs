namespace FinGuard.UI.Common;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public Dictionary<string, List<string>> Errors { get; private set; } = new();

    public static ServiceResult<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public static ServiceResult<T> Failure(Dictionary<string, List<string>> errors) =>
        new() { IsSuccess = false, Errors = errors };

    public static ServiceResult<T> Failure(string globalError) => new()
    {
        IsSuccess = false,
        Errors = new Dictionary<string, List<string>> { { string.Empty, new List<string> { globalError } } }
    };
}
