namespace FinGuard.Application.Commons.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
