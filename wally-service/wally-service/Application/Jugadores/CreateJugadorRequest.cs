namespace WallyBallBackend.Application.Jugadores;

public sealed record CreateJugadorRequest(
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento);
