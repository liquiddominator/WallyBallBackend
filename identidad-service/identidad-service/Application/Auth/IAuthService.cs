namespace IdentidadService.Application.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<AuthResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);

    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);

    Task<AuthUserResponse?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken);

    Task<AuthOperationResult> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken);
}
