namespace WallyBallBackend.Infrastructure.Personas;

public sealed record CreateJugadorPersonaRequest(
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento,
    string Email,
    string Password);
