namespace PersonasService.Application.Personas;

public sealed record PersonaResponse(
    int IdPersona,
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento,
    bool Activo);

public sealed record JugadorPersonaResponse(
    int IdPersona,
    int IdUsuario,
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento,
    string Email,
    IReadOnlyCollection<string> Roles);
