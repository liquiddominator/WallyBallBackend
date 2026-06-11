namespace PersonasService.Application.Gestion;

public sealed record GestionUserResponse(
    int IdUsuario,
    string Email,
    string? NombreCompleto,
    bool Activo,
    int AccessFailedCount,
    DateTime? LockoutEndUtc,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion,
    DateTime? PasswordChangedAtUtc,
    IReadOnlyCollection<string> Roles);
