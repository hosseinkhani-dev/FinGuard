namespace FinGuard.UI.Services.Auth;

public interface ITokenService
{
    Task<bool> RefreshAsync();
}
