namespace IdentidadService.Application.Auth;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    AuthUserResponse User);

public sealed record AuthUserResponse(
    int IdUsuario,
    string Email,
    string? NombreCompleto,
    IReadOnlyCollection<string> Roles);
