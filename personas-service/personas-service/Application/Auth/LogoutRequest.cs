namespace PersonasService.Application.Auth;

public sealed record LogoutRequest(
    string? RefreshToken);
