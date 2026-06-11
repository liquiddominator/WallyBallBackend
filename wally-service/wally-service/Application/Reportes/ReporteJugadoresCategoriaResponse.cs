namespace WallyBallBackend.Application.Reportes;

public sealed record ReporteJugadoresCategoriaResponse(
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    int TotalJugadores,
    IReadOnlyCollection<ReporteJugadoresEquipoResponse> Equipos);

public sealed record ReporteJugadoresEquipoResponse(
    int IdEquipo,
    string Equipo,
    int TotalJugadores,
    IReadOnlyCollection<ReporteJugadorResponse> Jugadores);

public sealed record ReporteJugadorResponse(
    int IdJugador,
    int? IdPersona,
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateOnly? FechaNacimiento,
    string EstadoInscripcion,
    DateTime FechaInscripcion);
