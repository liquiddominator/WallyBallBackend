namespace ReportesService.Application.Reportes;

public sealed record ReporteEquiposCategoriaResponse(
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    int TotalEquipos,
    int TotalJugadoresActivos,
    IReadOnlyCollection<ReporteEquipoResponse> Equipos);

public sealed record ReporteEquipoResponse(
    int IdEquipo,
    string Equipo,
    bool Activo,
    int CantidadJugadoresActivos,
    DateTime FechaCreacion);

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

public sealed record ReporteResultadoResponse(
    int IdResultado,
    int IdPartido,
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    int IdFase,
    string Fase,
    int IdJornada,
    int NumeroJornada,
    int IdEquipoLocal,
    string EquipoLocal,
    int IdEquipoVisitante,
    string EquipoVisitante,
    int SetsLocal,
    int SetsVisitante,
    int IdEquipoGanador,
    string EquipoGanador,
    DateTime FechaRegistro,
    DateTime? FechaActualizacion,
    IReadOnlyCollection<ResultadoSetResponse> Sets);

public sealed record ResultadoSetResponse(
    int NumeroSet,
    int PuntosLocal,
    int PuntosVisitante,
    int IdEquipoGanador,
    string EquipoGanador);

public sealed record ReportePosicionesCategoriaResponse(
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    DateTime? FechaActualizacion,
    IReadOnlyCollection<PosicionResponse> Posiciones);

public sealed record PosicionResponse(
    int Posicion,
    int IdEquipo,
    string Equipo,
    int PartidosJugados,
    int Ganados,
    int Perdidos,
    int SetsFavor,
    int SetsContra,
    int DiferenciaSets,
    int PuntosFavor,
    int PuntosContra,
    int DiferenciaPuntos,
    int Puntos,
    DateTime FechaActualizacion);
