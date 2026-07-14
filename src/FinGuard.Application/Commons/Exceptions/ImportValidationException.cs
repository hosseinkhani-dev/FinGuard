namespace FinGuard.Application.Commons.Exceptions;

public class ImportValidationException : AppException
{
    public ImportValidationException(string message) : base(message) { }
}
