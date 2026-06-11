namespace WallyBallBackend.Application.Jugadores;

public sealed record CreateJugadorRequest(
    string Cedula,
    string Nombre,
    string Apellido,
    string Email,
    string PasswordTemporal,
    string? Telefono,
    DateOnly? FechaNacimiento);
