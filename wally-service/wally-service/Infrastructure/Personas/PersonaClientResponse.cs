namespace WallyBallBackend.Infrastructure.Personas;

public sealed record PersonaClientResponse(
    int IdPersona,
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento,
    bool Activo);

public sealed record JugadorPersonaClientResponse(
    int IdPersona,
    int IdUsuario,
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento,
    string Email,
    IReadOnlyCollection<string> Roles);
