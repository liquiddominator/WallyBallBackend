namespace IdentidadService.Application.Auth;

public sealed record LogoutRequest(
    string? RefreshToken);
