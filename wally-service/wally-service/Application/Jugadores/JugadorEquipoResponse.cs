namespace WallyBallBackend.Application.Jugadores;

public sealed record JugadorEquipoResponse(
    int IdInscripcion,
    int IdJugador,
    int IdEquipo,
    string EquipoNombre,
    int IdCampeonatoCategoria,
    string CategoriaNombre,
    int IdCampeonato,
    string CampeonatoNombre,
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento,
    string Estado,
    DateTime FechaInscripcion);
