namespace FinGuard.Application.Commons.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
