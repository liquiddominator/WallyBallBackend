namespace IdentidadService.Application.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string? NombreCompleto);
