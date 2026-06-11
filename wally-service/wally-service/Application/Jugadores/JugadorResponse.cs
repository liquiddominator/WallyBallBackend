namespace WallyBallBackend.Application.Jugadores;

public sealed record JugadorResponse(
    int IdJugador,
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion,
    IReadOnlyCollection<JugadorEquipoResumenResponse> Equipos);

public sealed record JugadorEquipoResumenResponse(
    int IdInscripcion,
    int IdEquipo,
    string EquipoNombre,
    int IdCampeonatoCategoria,
    string CategoriaNombre,
    int IdCampeonato,
    string CampeonatoNombre,
    string Estado,
    DateTime FechaInscripcion);
